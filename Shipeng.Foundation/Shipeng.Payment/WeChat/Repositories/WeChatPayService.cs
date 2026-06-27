using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Shipeng.Payment.WeChat
{
    /// <summary>
    /// 微信支付 V3 基础封装服务。
    ///
    /// 重要定位：
    /// 1. 这里只封装微信支付基础能力。
    /// 2. 不写给前端直接调用的 API。
    /// 3. 不写认养订单、商城订单、服务订单、权益、库存、分账等业务逻辑。
    /// 4. 业务模块需要支付时，先创建本地支付单，再调用 CreatePayAsync。
    /// 5. 业务模块收到微信回调时，读取原始 body 和 headers，再调用 ParsePayNotify 或 ParseRefundNotify。
    /// </summary>
    public class WeChatPayService : IWeChatPayService
    {
        private readonly HttpClient _httpClient;
        private readonly WeChatPayOptions _options;

        /// <summary>
        /// 构造方法。
        /// </summary>
        /// <param name="httpClient">HTTP 客户端，用于请求微信支付接口</param>
        /// <param name="options">微信支付配置</param>
        public WeChatPayService(HttpClient httpClient, IOptions<WeChatPayOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        /// <summary>
        /// 创建微信支付订单。
        ///
        /// 支持：
        /// 1. JSAPI/小程序支付：返回 prepay_id 和前端调起支付参数。
        /// 2. Native 扫码支付：返回 code_url，业务层可生成二维码。
        ///
        /// 注意：
        /// 1. outTradeNo 必须是本系统唯一支付单号。
        /// 2. totalFee 单位是分，不是元。
        /// 3. JSAPI/小程序支付必须传 openId。
        /// 4. 这里只请求微信下单，不修改本地业务订单。
        /// </summary>
        public async Task<CreateWeChatPayDto> CreatePayAsync(CreateWeChatPayRequestModel input)
        {
            ValidateOptions();

            if (input == null) throw new Exception("微信支付参数不能为空！");
            if (string.IsNullOrWhiteSpace(input.outTradeNo)) throw new Exception("商户订单号不能为空！");
            if (string.IsNullOrWhiteSpace(input.description)) throw new Exception("订单标题不能为空！");
            if (input.totalFee <= 0) throw new Exception("支付金额必须大于0！");
            if (input.tradeType == WeChatPayTradeTypeEnum.JsApi && string.IsNullOrWhiteSpace(input.openId))
                throw new Exception("JSAPI/小程序支付 openId 不能为空！");

            var url = input.tradeType == WeChatPayTradeTypeEnum.Native
                ? "/v3/pay/transactions/native"
                : "/v3/pay/transactions/jsapi";

            var bodyObj = new Dictionary<string, object>
            {
                ["appid"] = _options.appId,
                ["mchid"] = _options.mchId,
                ["description"] = input.description,
                ["out_trade_no"] = input.outTradeNo,
                ["notify_url"] = _options.notifyUrl,
                ["amount"] = new { total = input.totalFee, currency = "CNY" }
            };

            if (input.tradeType == WeChatPayTradeTypeEnum.JsApi)
                bodyObj["payer"] = new { openid = input.openId };

            var body = JsonSerializer.Serialize(bodyObj);
            var result = await SendAsync(HttpMethod.Post, url, body);

            using var doc = JsonDocument.Parse(result);
            var root = doc.RootElement;

            var dto = new CreateWeChatPayDto
            {
                outTradeNo = input.outTradeNo
            };

            if (input.tradeType == WeChatPayTradeTypeEnum.Native)
            {
                dto.codeUrl = root.TryGetProperty("code_url", out var codeUrl) ? codeUrl.GetString() : "";
                return dto;
            }

            dto.prepayId = root.TryGetProperty("prepay_id", out var prepayId) ? prepayId.GetString() : "";
            if (string.IsNullOrWhiteSpace(dto.prepayId))
                throw new Exception("微信支付下单成功，但未返回 prepay_id！");

            dto.jsApiPay = BuildJsApiPay(dto.prepayId);
            return dto;
        }

        /// <summary>
        /// 根据商户订单号查询微信支付订单。
        /// 用于回调未收到、回调异常、支付状态补偿、对账核对。
        /// </summary>
        public async Task<string> QueryByOutTradeNoAsync(string outTradeNo)
        {
            ValidateOptions();

            if (string.IsNullOrWhiteSpace(outTradeNo))
                throw new Exception("商户订单号不能为空！");

            var safeOutTradeNo = Uri.EscapeDataString(outTradeNo);
            var url = $"/v3/pay/transactions/out-trade-no/{safeOutTradeNo}?mchid={_options.mchId}";

            return await SendAsync(HttpMethod.Get, url, "");
        }

        /// <summary>
        /// 根据微信支付订单号查询微信支付订单。
        /// 已经拿到 transaction_id 时可以用这个查微信侧交易状态。
        /// </summary>
        public async Task<string> QueryByTransactionIdAsync(string transactionId)
        {
            ValidateOptions();

            if (string.IsNullOrWhiteSpace(transactionId))
                throw new Exception("微信支付订单号不能为空！");

            var safeTransactionId = Uri.EscapeDataString(transactionId);
            var url = $"/v3/pay/transactions/id/{safeTransactionId}?mchid={_options.mchId}";

            return await SendAsync(HttpMethod.Get, url, "");
        }

        /// <summary>
        /// 关闭未支付微信支付订单。
        /// 已支付订单不能关单，已支付要走退款。
        /// </summary>
        public async Task CloseAsync(string outTradeNo)
        {
            ValidateOptions();

            if (string.IsNullOrWhiteSpace(outTradeNo))
                throw new Exception("商户订单号不能为空！");

            var safeOutTradeNo = Uri.EscapeDataString(outTradeNo);
            var url = $"/v3/pay/transactions/out-trade-no/{safeOutTradeNo}/close";

            var body = JsonSerializer.Serialize(new
            {
                mchid = _options.mchId
            });

            await SendAsync(HttpMethod.Post, url, body);
        }

        /// <summary>
        /// 申请微信退款。
        ///
        /// 注意：
        /// 1. 这里只请求微信退款，不修改本地退款单。
        /// 2. 是否允许退款、退款金额是否正确，由业务模块先校验。
        /// 3. outRefundNo 必须是本系统唯一退款单号。
        /// 4. 金额单位都是分。
        /// </summary>
        public async Task<string> RefundAsync(CreateWeChatRefundRequestModel input)
        {
            ValidateOptions();

            if (input == null) throw new Exception("微信退款参数不能为空！");
            if (string.IsNullOrWhiteSpace(input.outTradeNo) && string.IsNullOrWhiteSpace(input.transactionId))
                throw new Exception("商户订单号和微信支付订单号不能同时为空！");
            if (string.IsNullOrWhiteSpace(input.outRefundNo)) throw new Exception("商户退款单号不能为空！");
            if (input.refundFee <= 0) throw new Exception("退款金额必须大于0！");
            if (input.totalFee <= 0) throw new Exception("原订单金额必须大于0！");
            if (input.refundFee > input.totalFee) throw new Exception("退款金额不能大于原订单金额！");

            var bodyObj = new Dictionary<string, object>
            {
                ["out_refund_no"] = input.outRefundNo,
                ["reason"] = input.reason ?? "",
                ["amount"] = new
                {
                    refund = input.refundFee,
                    total = input.totalFee,
                    currency = "CNY"
                }
            };

            if (!string.IsNullOrWhiteSpace(input.transactionId))
                bodyObj["transaction_id"] = input.transactionId;
            else
                bodyObj["out_trade_no"] = input.outTradeNo;

            if (!string.IsNullOrWhiteSpace(input.notifyUrl))
                bodyObj["notify_url"] = input.notifyUrl;

            var body = JsonSerializer.Serialize(bodyObj);
            return await SendAsync(HttpMethod.Post, "/v3/refund/domestic/refunds", body);
        }

        /// <summary>
        /// 根据商户退款单号查询退款状态。
        /// 用于退款回调未收到、退款状态补偿、人工排查。
        /// </summary>
        public async Task<string> QueryRefundByOutRefundNoAsync(string outRefundNo)
        {
            ValidateOptions();

            if (string.IsNullOrWhiteSpace(outRefundNo))
                throw new Exception("商户退款单号不能为空！");

            var safeOutRefundNo = Uri.EscapeDataString(outRefundNo);
            var url = $"/v3/refund/domestic/refunds/{safeOutRefundNo}";

            return await SendAsync(HttpMethod.Get, url, "");
        }

        /// <summary>
        /// 解析微信支付成功回调。
        ///
        /// 这里只做：
        /// 1. 验签。
        /// 2. 解密。
        /// 3. 反序列化为支付结果。
        ///
        /// 不做：
        /// 1. 不查询本地支付单。
        /// 2. 不修改业务订单。
        /// 3. 不生成权益。
        /// 4. 不扣库存。
        /// </summary>
        public WeChatPayNotifyResourceDto ParsePayNotify(IDictionary<string, string> headers, string body)
        {
            var resourceJson = ParseNotifyResourceJson(headers, body);

            var resource = JsonSerializer.Deserialize<WeChatPayNotifyResourceDto>(resourceJson);
            if (resource == null)
                throw new Exception("微信支付回调解密结果解析失败！");

            return resource;
        }

        /// <summary>
        /// 解析微信退款回调。
        ///
        /// 这里只做：
        /// 1. 验签。
        /// 2. 解密。
        /// 3. 反序列化为退款结果。
        ///
        /// 不做本地退款单状态更新，业务模块自己处理。
        /// </summary>
        public WeChatRefundNotifyResourceDto ParseRefundNotify(IDictionary<string, string> headers, string body)
        {
            var resourceJson = ParseNotifyResourceJson(headers, body);

            var resource = JsonSerializer.Deserialize<WeChatRefundNotifyResourceDto>(resourceJson);
            if (resource == null)
                throw new Exception("微信退款回调解密结果解析失败！");

            return resource;
        }

        /// <summary>
        /// 解析微信回调 resource 明文 JSON。
        /// 支付回调和退款回调外层结构一致，所以统一封装。
        /// </summary>
        private string ParseNotifyResourceJson(IDictionary<string, string> headers, string body)
        {
            ValidateOptions();

            if (headers == null || headers.Count == 0)
                throw new Exception("微信回调请求头不能为空！");
            if (string.IsNullOrWhiteSpace(body))
                throw new Exception("微信回调内容不能为空！");

            var timestamp = GetHeaderValue(headers, "Wechatpay-Timestamp");
            var nonce = GetHeaderValue(headers, "Wechatpay-Nonce");
            var signature = GetHeaderValue(headers, "Wechatpay-Signature");

            if (string.IsNullOrWhiteSpace(timestamp)
                || string.IsNullOrWhiteSpace(nonce)
                || string.IsNullOrWhiteSpace(signature))
                throw new Exception("微信回调签名请求头不完整！");

            if (!string.IsNullOrWhiteSpace(_options.platformCertificatePath))
            {
                var verifyOk = VerifyNotifySignature(timestamp, nonce, body, signature);
                if (!verifyOk)
                    throw new Exception("微信回调验签失败！");
            }

            var notify = JsonSerializer.Deserialize<WeChatPayNotifyRequestModel>(body);
            if (notify == null || notify.resource == null)
                throw new Exception("微信回调 resource 不能为空！");

            if (string.IsNullOrWhiteSpace(notify.resource.ciphertext))
                throw new Exception("微信回调 ciphertext 不能为空！");

            return DecryptNotifyResource(
                notify.resource.associated_data,
                notify.resource.nonce,
                notify.resource.ciphertext);
        }

        /// <summary>
        /// 验证微信支付/退款回调签名。
        /// 验签使用微信支付平台证书公钥，不是商户 API 证书。
        /// </summary>
        public bool VerifyNotifySignature(string timestamp, string nonce, string body, string signature)
        {
            if (string.IsNullOrWhiteSpace(timestamp)) return false;
            if (string.IsNullOrWhiteSpace(nonce)) return false;
            if (string.IsNullOrWhiteSpace(body)) return false;
            if (string.IsNullOrWhiteSpace(signature)) return false;

            var certPath = ResolvePath(_options.platformCertificatePath);
            if (string.IsNullOrWhiteSpace(certPath) || !File.Exists(certPath))
                throw new Exception("微信支付平台证书文件不存在，无法验证回调签名！");

            var message = $"{timestamp}\n{nonce}\n{body}\n";
            var data = Encoding.UTF8.GetBytes(message);
            var signBytes = Convert.FromBase64String(signature);

            using var certificate = X509Certificate2.CreateFromPemFile(certPath);
            using var rsa = certificate.GetRSAPublicKey();

            if (rsa == null)
                throw new Exception("微信支付平台证书中没有可用的 RSA 公钥！");

            return rsa.VerifyData(data, signBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        /// <summary>
        /// 解密微信支付/退款回调 resource。
        /// 使用 APIv3 密钥 AES-256-GCM 解密。
        /// </summary>
        public string DecryptNotifyResource(string associatedData, string nonce, string ciphertext)
        {
            ValidateOptions();

            if (string.IsNullOrWhiteSpace(nonce))
                throw new Exception("微信回调 nonce 不能为空！");
            if (string.IsNullOrWhiteSpace(ciphertext))
                throw new Exception("微信回调 ciphertext 不能为空！");

            var key = Encoding.UTF8.GetBytes(_options.apiV3Key);
            if (key.Length != 32)
                throw new Exception("微信支付 APIv3 密钥必须是32字节！");

            var nonceBytes = Encoding.UTF8.GetBytes(nonce);
            var associatedDataBytes = Encoding.UTF8.GetBytes(associatedData ?? "");
            var cipherBytes = Convert.FromBase64String(ciphertext);

            if (cipherBytes.Length <= 16)
                throw new Exception("微信回调 ciphertext 格式不正确！");

            var tag = cipherBytes[^16..];
            var data = cipherBytes[..^16];
            var plaintext = new byte[data.Length];

            using var aesGcm = new AesGcm(key, 16);
            aesGcm.Decrypt(nonceBytes, data, tag, plaintext, associatedDataBytes);

            return Encoding.UTF8.GetString(plaintext);
        }

        /// <summary>
        /// 构建微信回调成功响应。
        /// 业务接口处理成功后直接返回这个对象。
        /// </summary>
        public object BuildNotifySuccess()
        {
            return new
            {
                code = "SUCCESS",
                message = "成功"
            };
        }

        /// <summary>
        /// 构建微信回调失败响应。
        /// 返回 FAIL 后，微信会继续重试通知。
        /// </summary>
        public object BuildNotifyFail(string message)
        {
            return new
            {
                code = "FAIL",
                message = string.IsNullOrWhiteSpace(message) ? "失败" : message
            };
        }

        /// <summary>
        /// 发送微信支付 V3 HTTP 请求。
        /// </summary>
        private async Task<string> SendAsync(HttpMethod method, string url, string body)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("微信支付请求地址不能为空！");

            body ??= "";

            var request = new HttpRequestMessage(method, "https://api.mch.weixin.qq.com" + url);

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", BuildAuthorization(method.Method, url, body));

            if (method != HttpMethod.Get)
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"微信支付请求失败，HTTP状态码：{(int)response.StatusCode}，返回内容：{result}");

            return result;
        }

        /// <summary>
        /// 构建微信支付 V3 Authorization 请求头。
        /// 每次请求微信支付 V3 接口都要签名。
        /// </summary>
        private string BuildAuthorization(string method, string url, string body)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonce = Guid.NewGuid().ToString("N");

            var message = $"{method}\n{url}\n{timestamp}\n{nonce}\n{body}\n";
            var signature = Sign(message);

            return $"WECHATPAY2-SHA256-RSA2048 mchid=\"{_options.mchId}\",nonce_str=\"{nonce}\",timestamp=\"{timestamp}\",serial_no=\"{_options.serialNo}\",signature=\"{signature}\"";
        }

        /// <summary>
        /// 使用商户 API 私钥进行 RSA-SHA256 签名。
        /// </summary>
        private string Sign(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new Exception("微信支付签名内容不能为空！");

            var privateKeyPath = ResolvePath(_options.privateKeyPath);
            if (string.IsNullOrWhiteSpace(privateKeyPath) || !File.Exists(privateKeyPath))
                throw new Exception($"微信支付商户 API 私钥文件不存在：{privateKeyPath}");

            var privateKeyText = File.ReadAllText(privateKeyPath);

            using var rsa = RSA.Create();

            if (privateKeyText.Contains("BEGIN RSA PRIVATE KEY"))
            {
                var key = privateKeyText
                    .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                    .Replace("-----END RSA PRIVATE KEY-----", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Trim();

                rsa.ImportRSAPrivateKey(Convert.FromBase64String(key), out _);
            }
            else
            {
                var key = privateKeyText
                    .Replace("-----BEGIN PRIVATE KEY-----", "")
                    .Replace("-----END PRIVATE KEY-----", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Trim();

                rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(key), out _);
            }

            var data = Encoding.UTF8.GetBytes(message);
            var sign = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(sign);
        }

        /// <summary>
        /// 构建 JSAPI/小程序前端调起支付参数。
        /// 这个方法是封装内部能力，业务层创建支付单后可把结果返回给前端。
        /// </summary>
        private WeChatJsApiPayDto BuildJsApiPay(string prepayId)
        {
            if (string.IsNullOrWhiteSpace(prepayId))
                throw new Exception("微信支付 prepay_id 不能为空！");

            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var nonceStr = Guid.NewGuid().ToString("N");
            var package = $"prepay_id={prepayId}";

            var message = $"{_options.appId}\n{timeStamp}\n{nonceStr}\n{package}\n";

            return new WeChatJsApiPayDto
            {
                appId = _options.appId,
                timeStamp = timeStamp,
                nonceStr = nonceStr,
                package = package,
                signType = "RSA",
                paySign = Sign(message)
            };
        }

        /// <summary>
        /// 忽略大小写获取 Header 值。
        /// </summary>
        private static string GetHeaderValue(IDictionary<string, string> headers, string key)
        {
            var item = headers.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            return item.Value ?? "";
        }

        /// <summary>
        /// 解析证书或私钥文件路径。
        /// 支持绝对路径和相对路径。
        /// </summary>
        private static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";

            if (Path.IsPathRooted(path)) return path;

            var basePath = Path.Combine(AppContext.BaseDirectory, path);
            if (File.Exists(basePath)) return basePath;

            return Path.Combine(Directory.GetCurrentDirectory(), path);
        }

        /// <summary>
        /// 校验微信支付基础配置。
        /// </summary>
        private void ValidateOptions()
        {
            if (string.IsNullOrWhiteSpace(_options.appId))
                throw new Exception("微信支付 appId 未配置！");

            if (string.IsNullOrWhiteSpace(_options.mchId))
                throw new Exception("微信支付 mchId 未配置！");

            if (string.IsNullOrWhiteSpace(_options.serialNo))
                throw new Exception("微信支付商户 API 证书序列号 serialNo 未配置！");

            if (string.IsNullOrWhiteSpace(_options.privateKeyPath))
                throw new Exception("微信支付商户 API 私钥路径 privateKeyPath 未配置！");

            if (string.IsNullOrWhiteSpace(_options.apiV3Key))
                throw new Exception("微信支付 APIv3 密钥 apiV3Key 未配置！");

            if (Encoding.UTF8.GetBytes(_options.apiV3Key).Length != 32)
                throw new Exception("微信支付 APIv3 密钥必须是32字节！");

            if (string.IsNullOrWhiteSpace(_options.notifyUrl))
                throw new Exception("微信支付回调地址 notifyUrl 未配置！");
        }
    }
}
using Shipeng;
using Shipeng.DataValidation;
using Shipeng.Dependency;
using Shipeng.FriendlyException;
using Shipeng.UnifyResult;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// �Ѻ��쳣������
    /// </summary>
    [SuppressSniffer]
    public sealed class FriendlyExceptionFilter : IAsyncExceptionFilter
    {
        /// <summary>
        /// �쳣����
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            // �����쳣�������ʵ���Զ����쳣������������¼��־��
            var globalExceptionHandler = context.HttpContext.RequestServices.GetService<IGlobalExceptionHandler>();
            if (globalExceptionHandler != null)
            {
                await globalExceptionHandler.OnExceptionAsync(context);
            }

            // ����쳣�������ط�������˴������ô���ﲻ�ٴ���
            if (context.ExceptionHandled) return;

            // ��ȡ��������Ϣ
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            // �����쳣��Ϣ
            var exceptionMetadata = UnifyContext.GetExceptionMetadata(context);

            // �ж��Ƿ�����֤�쳣
            var isValidationException = context.Exception is AppFriendlyException friendlyException && friendlyException.ValidationException;

            // �ж��Ƿ������淶�����������ǣ���ֻ����Ϊ�Ѻ��쳣��Ϣ
            if (UnifyContext.CheckFailedNonUnify(actionDescriptor.MethodInfo, out var unifyResult))
            {
                // �������֤�쳣������ 400
                if (isValidationException) context.Result = new BadRequestResult();
                else
                {
                    // �����Ѻ��쳣
                    context.Result = new ContentResult()
                    {
                        Content = exceptionMetadata.Errors.ToString(),
                        StatusCode = exceptionMetadata.StatusCode
                    };
                }
            }
            else
            {
                // �ж��Ƿ�֧�� MVC �淶������
                if (!UnifyContext.CheckSupportMvcController(context.HttpContext, actionDescriptor, out _)) return;

                // ִ�й淶���쳣����
                context.Result = unifyResult.OnException(context, exceptionMetadata);
            }

            // �ж��쳣��Ϣ�Ƿ�����֤�쳣������������֤�쳣��ҵ���׳��쳣��
            if (isValidationException)
            {
                // ������֤��Ϣ
                var validationMetadata = ValidatorContext.GetValidationMetadata((context.Exception as AppFriendlyException).ErrorMessage);

                App.PrintToMiniProfiler("Validation", "Failed", $"Validation Failed:\r\n{validationMetadata.Message}", true);
            }
            else PrintToMiniProfiler(context.Exception);
        }

        /// <summary>
        /// ��ӡ���� MiniProfiler ��
        /// </summary>
        /// <param name="exception"></param>
        internal static void PrintToMiniProfiler(Exception exception)
        {
            // �ж��Ƿ�ע�� MiniProfiler ���
            if (App.Settings.InjectMiniProfiler != true) return;

            // ��ȡ�쳣��ջ
            var traceFrame = new StackTrace(exception, true).GetFrame(0);

            // ��ȡ������ļ���
            var exceptionFileName = traceFrame.GetFileName();

            // ��ȡ������к�
            var exceptionFileLineNumber = traceFrame.GetFileLineNumber();

            // ��ӡ�����ļ������к�
            if (!string.IsNullOrWhiteSpace(exceptionFileName) && exceptionFileLineNumber > 0)
            {
                App.PrintToMiniProfiler("errors", "Locator", $"{exceptionFileName}:line {exceptionFileLineNumber}", true);
            }

            // ��ӡ�����Ķ�ջ��Ϣ
            App.PrintToMiniProfiler("errors", "StackTrace", exception.ToString(), true);
        }
    }
}
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;



namespace MoexConnector
{
    public class ConnectorBase
    {
        /// <summary>
        /// Key SecurityId
        /// </summary>
        public ConcurrentDictionary<string, Security> Securities = new ConcurrentDictionary<string, Security>();

        

        public void SendLogMessage(Exception exception, [CallerMemberName] string MemberName = "",
                                                [CallerFilePath] string sourceFilePath = "",
                                                [CallerLineNumber] int sourceLineNumber = 0)
        {
            try
            {
                if (exception is ThreadAbortException)
                {
                    return;
                }
                if (exception is AggregateException)
                {
                    AggregateException httpError = (AggregateException)exception;

                    for (int i = 0; i < httpError.InnerExceptions.Count; i++)

                    {
                        Exception item = httpError.InnerExceptions[i];

                        if (item is NullReferenceException == false)
                        {
                            if (item.InnerException == null)
                            {
                                SendLogMessage(exception.ToString());

                            }
                            else
                            {
                                SendLogMessage(sourceFilePath + "; " + MemberName + "; " + sourceLineNumber.ToString() + ";\n    " + item.InnerException.Message + $" {exception.StackTrace}");
                            }
                        }

                    }
                }
                else
                {
                    SendLogMessage(sourceFilePath + "; " + MemberName + "; " + sourceLineNumber.ToString() + ";\n    " + exception.Message + $" {exception.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                SendLogMessage(exception.ToString());
                SendLogMessage(ex.ToString());
            }
        }

        // sending messages to up
        // Отправка сообщений на верх

        /// <summary>
        /// send log message
        /// отправить сообщение в лог
        /// </summary>
        public void SendLogMessage(string message)
        {
            LogMessageEvent?.Invoke(message);
           

        }

        public event Action<string> LogMessageEvent;
    }
}
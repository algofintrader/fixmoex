


using System.Runtime.CompilerServices;

namespace MoexConnector
{
    /// <summary>
    /// Интерфейс коннектора 
    /// </summary>
    public interface IConnector
    {
        #region Constructor, connect, parametrs

        /// <summary>
        /// Эмуляция поведения коннектора
        /// true - эмуляция включена к моекс не подключается, размещение и исполнение ордеров эмулируется, в том числе, эмулируется своя сделка по исполненному ордеру
        /// </summary>
        public bool Emulation { get; set; }

        /// <summary>
        /// Лимит транзакций за 1 секунду, ограничение накладываемое биржей и логином, коннектор, во избежание штрафа за транзации, должен поддерживать лимит
        /// Лимит передается коннектору в конструкторе
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Загружать историю тиков с начала торгового дня ?
        /// true - да, загружать, передается коннектору в конструкторе
        /// </summary>
        public bool LoadTicksFromStart { get; set; }



        /// <summary>
        /// Время, когда коннектор запущен
        /// </summary>
        public DateTime StartOfConnector { get; }


        /// <summary>
        /// Статус текущего подключения коннектора
        /// </summary>
        public ServerConnectStatus Status { get; set; }


        /// <summary>
        /// Инициализация подключения к бирже
        /// </summary>
        public void Connect();

        /// <summary>
        /// Отключение от биржи и освобождение ресурсов
        /// </summary>
        public void Dispose();


        /// <summary>
        /// Остановить коннектор и отключить от биржи
        /// </summary>
        public void Stop();


        /// <summary>
        /// Событие изменение статуса подключение коннектора к бирже                
        /// </summary>
        public event Action<ServerConnectStatus> ConnectStatusChangeEvent;

        /// <summary>
        /// Событие от коннектора на верх с сообщение об ошибках, информационные и т.д. (записываем в лог) 
        /// </summary>
        public event Action<string> LogMessageEvent;

        /// <summary>
        /// Обработка Exception в catch и направление собщения с текстом ошибки в событие LogMessageEvent
        /// </summary>
        /// <param name="exception">Ошибка</param>
        /// <param name="MemberName"> Меод в котором возникла ошибка</param>
        /// <param name="sourceFilePath">Имя файла</param>
        /// <param name="sourceLineNumber">Строка</param>
        public void SendLogMessage(Exception exception, [CallerMemberName] string MemberName = "",
                                               [CallerFilePath] string sourceFilePath = "",
                                               [CallerLineNumber] int sourceLineNumber = 0);
        
        #endregion

        #region Instrument, portfolio


        /// <summary>
        /// Получить инструмент по его уникальному ключу-коду
        /// </summary>
        /// <param name="isin"></param>
        /// <returns></returns>
        public Security? GetSecurityByIsin(string isin);


        /// <summary>
        /// Портфель денежный, получаемый с биржи
        /// </summary>
        public Portfolio Portfolio { get; }


        /// <summary>
        /// Событие изменние денежной позиции портфеля
        /// </summary>
        public event Action<Portfolio> UpdatePortfolio;

        /// <summary>
        /// Событие изменения позиций инструментов на площадке
        /// </summary>
        public event Action<PositionOnBoard> UpdatePosition;

        /// <summary>
        /// Событие изменений информации по инструменту
        /// </summary>
        public event Action<Security> UpdateSecurity;


        /// <summary>
        /// Событие, что все инструменты с биржи получены
        /// </summary>
        public event Action SecuritiesLoadedEvent;



        #endregion

        #region Depth (Стаканы)


        /// <summary>
        /// Отписка от стакана по инструменту
        /// </summary>
        /// <param name="security"></param>
        /// <param name="emulatorIsOn"></param>
        public void UnRegisterMarketDepth(Security security, bool emulatorIsOn);



        /// <summary>
        /// Подписаться на получение стакана по инструменту
        /// </summary>
        /// <param name="security">Инструмент</param>
        /// <param name="emulatorIsOn">true - котировки стакана в режиме эмуляции</param>
        public void RegisterMarketDepth(Security security, bool emulatorIsOn);

        /// <summary>
        /// Событие, сообщающее что коннектор получил от биржи все стаканы и теперь все стаканы в актуальном состоянии и нахоядться в оналйне
        /// </summary>
        public event Action MarketDepthLoadedEvent;

        /// <summary>
        /// Событие изменение стакана по инструменту, MarketDepth собранный стакан который изменен 
        /// </summary>
        public event Action<MarketDepth> MarketDepthChangeEvent;

        #endregion

        #region Tick (Поток обезличенный сделок)


        /// <summary>
        /// Отписка от тиков по инструменту
        /// </summary>
        /// <param name="security"></param>
        /// <param name="emulation"></param>
        public void UnRegisterTicks(Security security, bool emulation);


        /// <summary>
        /// Подписка на тики по инструменту
        /// </summary>
        /// <param name="security">Инструмент</param>
        /// <param name="emulation">true - тики эмулируются</param>
        public void TryRegisterTicks(Security security, bool emulation);


        /// <summary>
        /// Сообщение, что все тики (обезличенные сделки) загружены
        /// </summary>
        public event Action TicksLoadedEvent;

        /// <summary>
        /// Событие со словарем тиков, пришедших с биржи
        /// тик - обезличенная сделка
        /// key = SecurityID - уникальный биржевой цифровой код инструмента, value = список тиков по этому инструменту
        /// </summary>
        public event Action<Dictionary<string, List<Trade>>> NewTickCollectionEvent;

        /// <summary>
        /// Список тиков, на которыве есть подписка и коорые коннектор отдает дальше "на верх"
        /// Коннектор получает с биржи все тики, но отправляет на верх только те, на которые есть подписка
        /// </summary>
        public HashSet<string> RegisteredTicks { get; }


        #endregion

        #region Trading

        /// <summary>
        /// Выставить (послать) ордер на биржу
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Task<string> ExecuteOrderAsync(Order order);

        /// <summary>
        /// Отменить (снять) ордер с рынка, по его биржевому номеру
        /// </summary>
        /// <param name="order">Ордер, в котором обязательно указан биржевой номер ордера</param>
        public void CancelOrderByExchangeId(Order order);

        /// <summary>
        /// Отменить (снять) ордер на рынке по клиентскому номеру ордера
        /// </summary>
        /// <param name="numberUser">Клиентский номер ордера</param>
        /// <returns></returns>
        public Task<bool> CancelOrder(int numberUser);


        /// <summary>
        /// Событие что все свои ордера с биржи с начала торгов получены и поток информации о своих  оредрах актуален и в онлайне
        /// </summary>
        public event Action OrderLoadedEvent;

        /// <summary>
        /// Событие изменение состояния ордера, (новые, отмененные, исполненные и т.д.), и сопутствующие сообщение от биржи или от коннектора, о причине изменения.
        /// </summary>
        public event Action<Order, string?> OrderChangedEvent;

        /// <summary>
        /// Событие своя сделка, MyTrade - сделка прошедная по своим ордерам 
        /// </summary>
        public event Action<MyTrade> NewMyTradeEvent;

        #endregion

    }
}
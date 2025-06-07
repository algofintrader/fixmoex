namespace MoexConnector
{
    /// <summary>
    /// Тип торговой площадки
    /// </summary>
    public enum BoardEnum
    {
        /// <summary>
        /// Фондовый рынок Московской биржи; 
        /// </summary>
        StockMoex,

        /// <summary>
        /// Срочный рынок Московской биржи; 
        /// </summary>
        FortsMoex,

        /// <summary>
        /// Валютный рынок Московской биржи; 
        /// </summary>
        CexMoex,

        /// <summary>
        /// CME, EUREX; 
        /// </summary>
        CME,

        /// <summary>
        /// NYSE, NASDAQ. 
        /// </summary>
        Nyse,

        /// <summary>
        /// Крипто
        /// </summary>
        Crypto,
    }



}

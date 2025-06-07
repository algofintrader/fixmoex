﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MoexConnector;

public class Level
{
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public string Direction { get; set; }
}

public class MarketDepth
{
    /// <summary>
    /// time to create a glass
    /// время создания стакана
    /// </summary>
    public DateTime Time;

    [JsonPropertyName("asks")]
    /// <summary>
    /// levels of sales. best with index 0
    /// уровни продаж. лучшая с индексом 0
    /// </summary>
    public List<MarketDepthLevel> Asks = [];

    [JsonPropertyName("bids")]
    /// <summary>
    /// purchase levels. best with index 0
    /// уровни покупок. лучшая с индексом 0
    /// </summary>
    public List<MarketDepthLevel> Bids = [];

    [JsonIgnore]
    /// <summary>
    /// total sales volume
    /// суммарный объём в продажах
    /// </summary>
    public decimal AskSummVolume
    {
        get
        {
            decimal vol = 0;
            for (int i = 0; Asks != null && i < Asks.Count; i++)
            {
                vol += Asks[i].Ask;
            }
            return vol;
        }
    }

    [JsonIgnore]
    /// <summary>
    /// total amount in purchases
    /// суммарный объём в покупках
    /// </summary>
    public decimal BidSummVolume
    {
        get
        {
            decimal vol = 0;
            for (int i = 0; Bids != null && i < Bids.Count; i++)
            {
                vol += Bids[i].Bid;
            }
            return vol;
        }
    }

    /// <summary>
    /// security that owns to glass
    /// бумага, которой принадлежит стакан
    /// </summary>
    public string SecurityId;

    /// <summary>
    /// set the cup from the stored value
    /// установить стакан из сохранённого значения
    /// </summary>
    public void SetMarketDepthFromString(string str)
    {
        string[] save = str.Split('_');

        int year =
        Convert.ToInt32(save[0][0].ToString() + save[0][1].ToString() + save[0][2].ToString() +
                  save[0][3].ToString());
        int month = Convert.ToInt32(save[0][4].ToString() + save[0][5].ToString());
        int day = Convert.ToInt32(save[0][6].ToString() + save[0][7].ToString());
        int hour = Convert.ToInt32(save[1][0].ToString() + save[1][1].ToString());
        int minute = Convert.ToInt32(save[1][2].ToString() + save[1][3].ToString());
        int second = Convert.ToInt32(save[1][4].ToString() + save[1][5].ToString());

        Time = new DateTime(year, month, day, hour, minute, second);

        Time = Time.AddMilliseconds(Convert.ToInt32(save[2]));

        
        string[] bids = save[3].Split('*');

        Asks = new List<MarketDepthLevel>();

        for (int i = 0; i < bids.Length - 1; i++)
        {
            string[] val = bids[i].Split('&');

            MarketDepthLevel newBid = new MarketDepthLevel();
            newBid.Ask = Convert.ToDecimal(val[0]);
            newBid.Price = Convert.ToDecimal(val[1]);
            Asks.Add(newBid);
        }

        string[] asks = save[4].Split('*');

        Bids = new List<MarketDepthLevel>();

        for (int i = 0; i < asks.Length - 1; i++)
        {
            string[] val = asks[i].Split('&');

            MarketDepthLevel newAsk = new MarketDepthLevel();
            newAsk.Bid = Convert.ToDecimal(val[0]);
            newAsk.Price = Convert.ToDecimal(val[1]);
            Bids.Add(newAsk);
        }
    }

    /// <summary>
    /// take the save string for the whole glass
    /// взять строку сохранения для всего стакана
    /// </summary>
    /// <param name="depth">depth of glass to keep/глубина стакана которую нужно сохранить</param>
    public string GetSaveStringToAllDepfh(int depth)
    {
        // NameSecurity_Time_Bids_Asks
        // Bids: level*level*level
        // level: Bid&Ask&Price

        if (depth == 0)
        {
            depth = 1;
        }

        string result = "";

        result += Time.ToString("yyyyMMdd_HHmmss") + "_";


        result += Time.Millisecond + "_"; 

        for (int i = 0; i < Asks.Count && i < depth; i++)
        {
            result += Asks[i].Ask + "&" + Asks[i].Price + "*";
        }
        result += "_";

        for (int i = 0; i < Bids.Count && i < depth; i++)
        {
            result += Bids[i].Bid + "&" + Bids[i].Price + "*";
        }

        return result;
    }

    /// <summary>
    /// take a "deep" copy of the glass
    /// взять "глубокую" копию стакана
    /// </summary>
    public MarketDepth GetCopy()
    {
        MarketDepth newDepth = new MarketDepth();
        newDepth.Time = Time;
        newDepth.SecurityId = SecurityId;
        newDepth.Asks = new List<MarketDepthLevel>();

        for (int i = 0; Asks != null && i < Asks.Count; i++)
        {
            var d = new MarketDepthLevel();
            newDepth.Asks.Add(d);
            try
            {
                d.Ask = Asks[i].Ask;
                d.Bid = Asks[i].Bid;
                d.Price = Asks[i].Price;
                
            }
            catch
            {
                //игнор 
            }
        }


        newDepth.Bids = new List<MarketDepthLevel>();

        for (int i = 0; Bids != null && i < Bids.Count; i++)
        {
            var d = new MarketDepthLevel();
            newDepth.Bids.Add(d);
            try
            {
                d.Ask = Bids[i].Ask;
                d.Bid = Bids[i].Bid;
                d.Price = Bids[i].Price;
                
            }
            catch
            {
                //игнор
            }
        }

        return newDepth;
    }
}

/// <summary>
/// class representing one price level in a glass
/// класс представляющий один ценовой уровень в стакане
/// </summary>
public class MarketDepthLevel :ICloneable
{

	//TODO: ПО хорошему это надо все рефакторить.
    //Но посыпится сборщик стаканов на стороне плазы, что не айс. 

	public static MarketDepthLevel GetEmptyLevel(decimal quotePrice)
	{
        return new MarketDepthLevel() { Ask = 0, Bid = 0, Price = quotePrice, Type = Side.Empty };
	}

    public object Clone()
    {
        return new MarketDepthLevel()
        {
            Ask = Ask,
            Bid = Bid,
            Price = Price,
            Type = Type,
            _side = _side,
            Id = Id,
        };
    }

    //[JsonIgnore]
    //[JsonPropertyName("a")]
    /// <summary>
    /// number of contracts for sale at this price level
    /// количество контрактов на продажу по этому уровню цены
    /// </summary>
    public decimal Ask { get; set; }

    //[JsonIgnore]
    //[JsonPropertyName("b")]
    /// <summary>
    /// \number of purchase contracts at this price level
    /// количество контрактов на покупку по этому уровню цены
    /// </summary>
    public decimal Bid { get; set; }

    public decimal Volume { get => Ask == 0 ? Bid : Ask; }

    public decimal VolumeSum
    {
	    get => Bid + Ask;
    }

    //public string VolumeString
    //{
    //    get
    //    {
    //        if (Ask != 0 && Bid !=0)
    //        {
    //            return Bid.ToString("####0.#####") + " | " + Ask.ToString("####0.#####");
    //        }
    //        else
    //        {
    //            return Volume.ToString("####0.#####");
    //        }
    //    }
    //}
    public string VolumeString()
    {
        if (Ask != 0 && Bid != 0)
        {
            return Bid.ToString("####0.#####") + " | " + Ask.ToString("####0.#####");
        }
        else
        {
            return Volume.ToString("####0.#####");
        }
    }

    private Side _side = Side.Empty;

    /// <summary>
    /// BID =0
    /// ASK =1
    /// </summary>
    public Side Type
    {
	    get
	    {
            //велосипед на велосипеде уже... 
            //TODO: исправить
		    if (_side != Side.Empty)
			    return _side;

		    if (Volume == 0)
			    return Side.Empty;

		    if (Bid != 0) return Side.Buy;

		    return Side.Sell;
	    }
	    set
	    {
		    _side = value;

	    }
    }

    

    /// <summary>
    /// price
    /// цена
    /// </summary>
    public decimal Price { get; set; }

    [JsonIgnore]
    /// <summary>
    /// Unique price level number, required for working with BitMex
    /// уникальный номер ценового уровня, необходим для работы с BitMex
    /// </summary>
    public long Id { get; set; }


    /// <summary>
    /// Выставленный объем на этом уровне... 
    /// </summary>
    public decimal? RestVolume { get; set; } = null;

}

public class ClusterLevel:INotifyPropertyChanged,ICloneable
{
	private decimal _bidVolume;
	private decimal _askVolume;


	public decimal BidVolume
	{
		get => _bidVolume;
		set
		{
			if (value == _bidVolume) return;
			_bidVolume = value;
			OnPropertyChanged();
		}
	}

	public decimal AskVolume
	{
		get => _askVolume;
		set
		{
			if (value == _askVolume) return;
			_askVolume = value;
			OnPropertyChanged();
		}
	}

	public decimal Price { get; set; }

	public decimal Volume => AskVolume + BidVolume;

   public event PropertyChangedEventHandler? PropertyChanged;

   protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
   {
	   PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
   }

   protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
   {
	   if (EqualityComparer<T>.Default.Equals(field, value)) return false;
	   field = value;
	   OnPropertyChanged(propertyName);
	   return true;
   }

   public object Clone()
   {
	   return new ClusterLevel()
	   {
           BidVolume = BidVolume,
           AskVolume = AskVolume,
           Price = Price,
	   };
   }
}

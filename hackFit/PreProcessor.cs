namespace hack;

public class PreProcessor
{
    private readonly int size;
    private readonly float coefficient;
    private readonly float maxPrice;
    public PreProcessor(int size, float coefficient, float maxPrice)
    {
        this.size = size;
        this.coefficient = coefficient;
        this.maxPrice = maxPrice;
    }
    
    public bool FilterByListSize(List<Row> list)
    {
        return list.Count <= size;
    }
    
    public bool FilterByCurrentPriceAverage(List<Row> list)
    {
        var averagePrice = list.Average(x => x.CurrentPrice);
        var maxPrice2 = list.Max(x => x.CurrentPrice);
        var minPrice = list.Min(x => x.CurrentPrice);

        return ((maxPrice2 * 1.0 / averagePrice) <= coefficient) && ((averagePrice * 1.0 / minPrice) <= coefficient);
    }
}
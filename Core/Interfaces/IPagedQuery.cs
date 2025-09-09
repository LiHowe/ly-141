namespace Core.Interfaces;

public interface IPagedQuery
{
    int PageIndex { get; set; }
    
    int PageSize { get; set; }
    
}
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Products;
using Task = erp.Module.BusinessObjects.Planning.Task;

namespace erp.Module.BusinessObjects.Common;

[ImageName("Images")]
public class Picture(Session session): BaseEntity(session)
{
    private Contact _contact;
    private Product _product;
    private SalesDocument _salesDocument;
    private Task _task;
    private MediaDataObject _mediaDataObject;
    private string _notes;
    
    [Association("Contact-Pictures")]
    public Contact Contact
    {
        get => _contact;
        set => SetPropertyValue(nameof(Contact), ref _contact, value);
    }

    [Association("Product-Pictures")]
    public Product Product
    {
        get => _product;
        set => SetPropertyValue(nameof(Product), ref _product, value);
    }
    
    [Association("SalesDocument-Pictures")]
    public SalesDocument SalesDocument
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(SalesDocument), ref _salesDocument, value);
    }
    
    [Association("Task-Pictures")]
    public Task Task
    {
        get => _task;
        set => SetPropertyValue(nameof(Task), ref _task, value);
    }
    
    public MediaDataObject MediaDataObject
    {
        get => _mediaDataObject;
        set => SetPropertyValue(nameof(MediaDataObject), ref _mediaDataObject, value);
    }
    
    [Size(1000)]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }
}
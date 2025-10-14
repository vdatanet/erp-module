using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Products;
using Task = erp.Module.BusinessObjects.Planning.Task;

namespace erp.Module.BusinessObjects.Common;

[ImageName("BO_FileAttachment")]
[FileAttachment(nameof(File))]
public class Attachment(Session session) : BaseEntity(session)
{
    private Contact _contact;
    private Product _product;
    private SalesDocument _salesDocument;
    private Task _task;
    private FileData _fileData;
    private string _description;

    [Association("Contact-Attachments")]
    public Contact Contact
    {
        get => _contact;
        set => SetPropertyValue(nameof(Contact), ref _contact, value);
    }

    [Association("Product-Attachments")]
    public Product Product
    {
        get => _product;
        set => SetPropertyValue(nameof(Product), ref _product, value);
    }
    
    [Association("SalesDocument-Attachments")]
    public SalesDocument SalesDocument
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(SalesDocument), ref _salesDocument, value);
    }

    [Association("Task-Attachments")]
    public Task Task
    {
        get => _task;
        set => SetPropertyValue(nameof(Task), ref _task, value);
    }

    public FileData FileData
    {
        get => _fileData;
        set => SetPropertyValue(nameof(FileData), ref _fileData, value);
    }

    [Size(1000)]
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }
}
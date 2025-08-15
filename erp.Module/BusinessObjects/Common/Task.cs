using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Products;

namespace erp.Module.BusinessObjects.Common;

[DefaultClassOptions]
[NavigationItem("Common")]
[ImageName("BO_Task")]
public class Task(Session session) : BaseEntity(session)
{
    private string _name;
    private string _description;
    //private string _status;
    //private string _priority;
    //private string _type;
    private DateTime _dueDate;
    private DateTime _startDate;
    private DateTime _endDate;
    private ApplicationUser _owner;
    private ApplicationUser _assignedTo;
    private ApplicationUser _completedBy;
    private Task _parentTask;
    private Contact _contact;
    private Product _product;
    private SalesDocument _salesDocument;
    private string _notes;

    [Size(255)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [Size(1000)]
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }

    public DateTime DueDate
    {
        get => _dueDate;
        set => SetPropertyValue(nameof(DueDate), ref _dueDate, value);
    }

    public DateTime StartDate
    {
        get => _startDate;
        set => SetPropertyValue(nameof(StartDate), ref _startDate, value);
    }

    public DateTime EndDate
    {
        get => _endDate;
        set => SetPropertyValue(nameof(EndDate), ref _endDate, value);
    }

    public ApplicationUser Owner
    {
        get => _owner;
        set => SetPropertyValue(nameof(Owner), ref _owner, value);
    }

    public ApplicationUser AssignedTo
    {
        get => _assignedTo;
        set => SetPropertyValue(nameof(AssignedTo), ref _assignedTo, value);
    }

    public ApplicationUser CompletedBy
    {
        get => _completedBy;
        set => SetPropertyValue(nameof(CompletedBy), ref _completedBy, value);
    }
    
    [Association("Task-Subtasks")]
    public Task ParentTask
    {
        get => _parentTask;
        set => SetPropertyValue(nameof(ParentTask), ref _parentTask, value);
    }

    [Association("Contact-Tasks")]
    public Contact Contact
    {
        get => _contact;
        set => SetPropertyValue(nameof(Contact), ref _contact, value);
    }

    [Association("Product-Tasks")]
    public Product Product
    {
        get => _product;
        set => SetPropertyValue(nameof(Product), ref _product, value);
    }
    
    [Association("SalesDocument-Tasks")]
    public SalesDocument SalesDocument
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(SalesDocument), ref _salesDocument, value);
    }

    [Size(1000)]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }

    [Aggregated]
    [Association("Task-Subtasks")] 
    public XPCollection<Task> Subtasks => GetCollection<Task>(nameof(Subtasks));

    [Aggregated]
    [Association("Task-Pictures")]
    public XPCollection<Picture> Pictures => GetCollection<Picture>(nameof(Pictures));
    
    [Aggregated]
    [Association("Task-Attachments")]
    public XPCollection<Attachment> Attachments => GetCollection<Attachment>(nameof(Attachments));
}
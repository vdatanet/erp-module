using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;
using Country = erp.Module.BusinessObjects.Common.Country;
using State = erp.Module.BusinessObjects.Common.State;
using Task = erp.Module.BusinessObjects.Common.Task;

namespace erp.Module.BusinessObjects.Contacts;

[DefaultClassOptions]
[NavigationItem("Contacts")]
[ImageName("BO_Contact")]
[DefaultProperty(nameof(Name))]
public class Contact(Session session) : BaseEntity(session)
{
    private string _name;
    private string _address;
    private Country _country;
    private State _state;
    private City _city;
    private string _phone;
    private string _email;
    private string _website;
    private MediaDataObject _picture;
    private string _notes;

    [Size(255)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [Size(255)]
    public string Address
    {
        get => _address;
        set => SetPropertyValue(nameof(Address), ref _address, value);
    }

    public Country Country
    {
        get => _country;
        set => SetPropertyValue(nameof(Country), ref _country, value);
    }

    [DataSourceProperty("Country.States")]
    public State State
    {
        get => _state;
        set => SetPropertyValue(nameof(State), ref _state, value);
    }

    [DataSourceProperty("State.Cities")]
    public City City
    {
        get => _city;
        set => SetPropertyValue(nameof(City), ref _city, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetPropertyValue(nameof(Phone), ref _phone, value);
    }

    public string Email
    {
        get => _email;
        set => SetPropertyValue(nameof(Email), ref _email, value);
    }

    public string Website
    {
        get => _website;
        set => SetPropertyValue(nameof(Website), ref _website, value);
    }
    
    public MediaDataObject Picture
    {
        get => _picture;
        set => SetPropertyValue(nameof(Picture), ref _picture, value);
    }
    
    [Size(1000)]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }
    
    [Aggregated]
    [Association("Contact-Tasks")]
    public XPCollection<Task> Tasks => GetCollection<Task>(nameof(Tasks));
    
    [Aggregated]
    [Association("Contact-Pictures")]
    public XPCollection<Picture> Pictures => GetCollection<Picture>(nameof(Pictures));
    
    [Aggregated]
    [Association("Contact-Attachments")]
    public XPCollection<Attachment> Attachments => GetCollection<Attachment>(nameof(Attachments));
}
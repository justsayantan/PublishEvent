# PublishEvent

Let’s create a sample event system project in web 8.

Tool Used:  Visual Studio 2013
1st:

Create a new Class Library project in Visual Studio.

Remember that you have to build your project with .Net 4.5.2 Framework.
2nd:

Add the references into your project as per your requirement. Here I have included below references –

using Tridion.Common;
using Tridion.ContentManager;
using Tridion.ContentManager.Publishing; (It is required when you want to publish any item from code)
using Tridion.ContentManager.Extensibility;
using Tridion.ContentManager.Extensibility.Events;
using Tridion.Logging; (It is required when you want to generate the log into the Event Viewer)
3rd:

Next you have to define the an unique attribute in your Event System class

[TcmExtension("Your Unique Class Name")].4th:Then create your class by extending the Tridion.ContentManager.Extensibility.TcmExtension class.namespace MyCustomEvent
{
[TcmExtension("PublishPageEvent")]
public class PublishPageEvent : TcmExtension
{
}
}
5th:

Now create the constructor and implement the signature of the Subscribe( Subscribe synchronously to an event) or SubscribeAsync (Subscribe asynchronously to an event) method in it. You can choose one of them as per the requirement.
There is no specific reason for it but I personally prefer to define that signature with in another method and call that method from my constructor.As you can see here I have created an additional MySubscribe() method and define the signature of Subcribe under that. Now I am calling that MySubscribe Method from my Constructor.

public PublishPageEvent()
{
MySubscribe ();
}


public void MySubscribe ()
{
//here I have define the signature of the Subcribe method
//Subscribe<TSubject, TEvent>(TcmEventHandler<TSubject, TEvent> eventHandler, EventPhases phases, EventSubscriptionOrder order)
//This signature is same for both Subscribe and SubscribeAsync method.

EventSystem.Subscribe<IdentifiableObject, SaveEventArgs>(OnPageSave, EventPhases.TransactionCommitted);
}
6th:

Your code structure is ready, now you have to implement the logic in that method

private void OnPageSave(IdentifiableObject subject, EventArgs e, EventPhases phase)
{
string pageId = subject.Id.ItemId.ToString();

//This will help you to generate the log into the Event Viewer

Logger.Write(String.Format("pageId: {0}", pageId), "PublishPageEvent", LoggingCategory.General, System.Diagnostics.TraceEventType.Information);try
{
Page page = subject as Page;
Session _session = page.Session;
Logger.Write(String.Format("Page _ID: {0}", page.Id.ToString()), "PublishPageEvent", LoggingCategory.General, System.Diagnostics.TraceEventType.Information);
IEnumerable<IdentifiableObject> items = new List<IdentifiableObject> { page };

/*in the previous version we have to define the publishing target before calling the publish method. Now in web 8 there is new concept introduced “Purpose” to manage all the publishing target settings directly into the Topology Manager. So In web 8 instead of defining the target you can also define the Purpose which I think more user-friendly and meaningful for a developer.*/


string[] purpose = new[] { "Staging" };

//Now you have to define the Publish Instruction detail

PublishInstruction instruction = new PublishInstruction(_session)
{
DeployAt = DateTime.Now,
MaximumNumberOfRenderFailures = 5,
RenderInstruction = new RenderInstruction(_session),
ResolveInstruction = new ResolveInstruction(_session),
StartAt = DateTime.MinValue
};


PublishEngine.Publish(items, instruction, purpose, PublishPriority.High);
Logger.Write(String.Format("Success Message: {0}", "Item send for Publishing"), "PublishPageEvent", LoggingCategory.General, System.Diagnostics.TraceEventType.Information);
}catch (Exception ex)
{
Logger.Write(String.Format("Exception Message: {0}", ex.Message), "PublishPageEvent", LoggingCategory.General, System.Diagnostics.TraceEventType.Information);
}
}
7th:

So you are now ready for event system code deployment into the CMS server.
Deployment Procedure:

    Copy the compiled DLL to the CMS Server under “<TRIDION_HOME>\bin\”
    Open the Tridion.ContentManager.config file from “<TRIDION_HOME>\config\” folder

Add the dll to the extensions:<add assemblyFileName=”<TRIDION_HOME>\bin\Your Event System Code.dll”/>

    Shut down COM+ service
    Restart the ‘Tridion Content Manager Service Host’ Service

Summary:

Hope this blog helps you to understand How to setup Event System Code in Web 8. I have also uploaded the code into GitHub. So you can download it from here.

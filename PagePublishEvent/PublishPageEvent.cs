using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tridion.ContentManager.Publishing;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.Logging;
using Tridion.ContentManager.Extensibility;
using Tridion.ContentManager.Extensibility.Events;
using System.Xml;

namespace PagePublishEvent
{
    [TcmExtension("PublishPageEvent")]
    public class PublishPageEvent : TcmExtension
    {
        public PublishPageEvent()
        {
            MySubscribe();
        }

        public void MySubscribe()
        {
            //here I have define the signature of the Subcribe method
            //Subscribe<TSubject, TEvent>(TcmEventHandler<TSubject, TEvent> eventHandler, EventPhases phases, EventSubscriptionOrder order)
            //This signature is same for both Subscribe and SubscribeAsync method.

            EventSystem.Subscribe<IdentifiableObject, SaveEventArgs>(OnComponentSave, EventPhases.TransactionCommitted);
        }

        private void OnComponentSave(IdentifiableObject subject, EventArgs e, EventPhases phase)
        {
            string pageId = subject.Id.ItemId.ToString();
            Logger.Write(String.Format("CompID: {0}", pageId), "PublishPageEvent", LoggingCategory.General, System.Diagnostics.TraceEventType.Information);
            try
            {
                Page page = subject as Page;
                Session _session = page.Session;

                //This will help you to generate the log into the Event Viewer
                Logger.Write(String.Format("CompID: {0}", page.Id.ToString()), "PublishPageEvent", LoggingCategory.General, System.Diagnostics.TraceEventType.Information);

                IEnumerable<IdentifiableObject> items = new List<IdentifiableObject> { page };

                /*in the previous version we have to define the publishing target before calling the publish method. 
                 * Now in web 8 there is new concept introduced “Purpose” to manage all the publishing target settings 
                 * directly into the Topology Manager. So In web 8 instead of defining the target you can also define 
                 * the Purpose which I think more user-friendly and meaningful for a developer.*/

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

                    //PublishEngine.Publish(IEnumerable<IdentifiableObject>  items,PublishInstruction publishInstruction,IEnumerable<string> purpose, PublishPriority publishPriority);
                    PublishEngine.Publish(items, instruction, purpose, PublishPriority.High);
                
            }
            catch (Exception ex)
            {
                Logger.Write(String.Format("Target: {0}", ex.Message), "PublishPageEvent", LoggingCategory.General, System.Diagnostics.TraceEventType.Information);
            }
        }
    }
}

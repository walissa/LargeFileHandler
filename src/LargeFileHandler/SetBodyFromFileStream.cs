using System;
using BizTalkComponents.Utils;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BizTalkComponents.PipelineComponents.LargeFileHandler
{
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Encoder)]
    [System.Runtime.InteropServices.Guid("d81e28ea-b2ac-4c8f-9958-c7c03e823490")]
    public partial class SetBodyFromFileStream : IBaseComponent, IComponent, IComponentUI, IPersistPropertyBag
    {
        public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            if (Disabled)
            {
                return pInMsg;
            }

            string errorMessage;
            if (!Validate(out errorMessage))
            {
                throw new ArgumentException(errorMessage);
            }
            string filePath = (string)pInMsg.Context.Read(new ContextProperty("http://schemas.microsoft.com/BizTalk/2003/file-properties#ReceivedFileName"));
            Task.Delay(500).GetAwaiter().GetResult();
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            pContext.ResourceTracker.AddResource(stream);
            pInMsg.BodyPart.Data = stream;
            return pInMsg;
        }
    }
}
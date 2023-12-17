using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting;
using System.IO;
using BizTalkComponents.Utils;

namespace BizTalkComponents.PipelineComponents.LargeFileHandler.Tests.UnitTests
{
    [TestClass]
    public class LargeFileHandlerTests
    {
        ContextProperty ctxReceivedFileName = new ContextProperty("http://schemas.microsoft.com/BizTalk/2003/file-properties#ReceivedFileName");
        [TestMethod]
        //Default behavior is to rename the file, and then delete it after reading it
        public void TestResetMessageBody()
        {
            var pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            var component = new ResetMessageBody { TempFolder = "TestLargeFile" };
            pipeline.AddComponent(component, PipelineStage.Decode);
            (string filePath, FileStream fs) = CreateLargeFile(100); //100MB
            var message = MessageHelper.CreateFromStream(fs);
            message.Context.Write(ctxReceivedFileName, filePath);
            var outputs = pipeline.Execute(message);
            var msg = outputs[0];
            string newFilePath = (string)msg.Context.Read(ctxReceivedFileName);
            var di = new DirectoryInfo(Path.GetDirectoryName(newFilePath));
            Assert.AreEqual(di.Name, component.TempFolder, true, "The TempFolder was not created.");
            Assert.IsTrue(File.Exists(newFilePath), "The file has not been moved to the temp folder.");
            Assert.IsFalse(File.Exists(filePath), "the original file still exists in the original location.");
            var stream = (MemoryStream)msg.BodyPart.GetOriginalDataStream();
            Assert.AreEqual(stream.Length, 0, "the message body is not empty.");
        }

        private (string filePath, FileStream fs) CreateLargeFile(int sizeMB)
        {
            string filePath = Path.GetTempFileName();
            var fs = new FileStream(filePath, FileMode.Create);
            var writer = new StreamWriter(fs, System.Text.Encoding.ASCII);
            var tempText = new string('A', 1048576);
            for (int i = 0; i < sizeMB; i++)
                writer.Write(tempText);
            writer.Flush();
            writer.Close();
            fs.Close();
            fs.Dispose();
            fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            return (filePath, fs);
        }

        [TestMethod]
        public void TestSetBodyFromFileStream()
        {
            var pipeline = PipelineFactory.CreateEmptySendPipeline();
            var component = new SetBodyFromFileStream();
            pipeline.AddComponent(component, PipelineStage.Encode);
            var message = MessageHelper.CreateFromString("");
            (string filePath, FileStream fs) = CreateLargeFile(100); //100MB
            fs.Close();
            message.Context.Write(new ContextProperty("http://schemas.microsoft.com/BizTalk/2003/file-properties#ReceivedFileName"), filePath);
            var output = pipeline.Execute(message);
            var stream = (FileStream)output.BodyPart.GetOriginalDataStream();
            Assert.AreEqual(stream.Length, 100 * 1024 * 1024, "The file size is not correct");
            stream.Close();
        }

        [TestMethod]
        public void TestResetBodyAndSetBodyFromFileStream()
        {
            var rcvPipeline = PipelineFactory.CreateEmptyReceivePipeline();
            var rcvComponent = new ResetMessageBody { TempFolder = "TestLargeFile" };
            rcvPipeline.AddComponent(rcvComponent, PipelineStage.Decode);

            var sndPipeline = PipelineFactory.CreateEmptySendPipeline();
            var sndComponent = new SetBodyFromFileStream();
            sndPipeline.AddComponent(sndComponent, PipelineStage.Encode);

            (string filePath, FileStream fs) = CreateLargeFile(100); //100MB
            var inMsg = MessageHelper.CreateFromStream(fs);
            inMsg.Context.Write(ctxReceivedFileName, filePath);
            var rcvOutput = rcvPipeline.Execute(inMsg)[0];
            Assert.IsFalse(File.Exists(filePath), "the original file still exists in the original location.");

            var sndOutput = sndPipeline.Execute(rcvOutput);

            string newFilePath = (string)sndOutput.Context.Read(ctxReceivedFileName);
            var di = new DirectoryInfo(Path.GetDirectoryName(newFilePath));
            Assert.AreEqual(di.Name, rcvComponent.TempFolder, true, "The TempFolder was not created.");
            Assert.IsTrue(File.Exists(newFilePath), "The file has not been moved to the temp folder.");


            var stream = (FileStream)sndOutput.BodyPart.GetOriginalDataStream();
            Assert.AreEqual(stream.Length, 100 * 1024 * 1024, "The file size is not correct");
            stream.Close();
        }
    }
}

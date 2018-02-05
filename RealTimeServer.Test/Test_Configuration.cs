using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RealTimeServer.Config;

namespace RealTimeServer.Test
{
    [TestFixture]
    public class Test_Configuration
    {
        public string defaultPath;

        [SetUp]
        public void Constructor()
        {
            defaultPath = "D:/UNITY/ExternalProjectsRandom/Real_Time_Server/RealTimeServer.Test/Assets/";
        }

        [Test]
        public void Test_ConfigFullParam()
        {
            Configuration.UpdateConfiguration(defaultPath + "config_fullparam.txt");
            Assert.That(Configuration.BindIpAddress, Is.EqualTo("127.0.0.1"));
            Assert.That(Configuration.BindPort, Is.EqualTo(2500));
            Assert.That(Configuration.Blocking, Is.EqualTo(false));
            Assert.That(Configuration.SendRate, Is.EqualTo(0.1f).Within(0.0001f));
            Assert.That(Configuration.ReceiveMaxBufferSize, Is.EqualTo(128));
            Assert.That(Configuration.NOfRefusePerClient, Is.EqualTo(5));
            Assert.That(Configuration.LogOn, Is.EqualTo(true));

        }
        [Test]
        public void Test_ConfigParser()
        {
            Configuration.UpdateConfiguration(defaultPath+"config1.txt");
            Assert.That(Configuration.HeaderSize, Is.EqualTo(8));
        }
        [Test]
        public void Test_ConfigParserComment()
        {
            Assert.That(() => Configuration.UpdateConfiguration(defaultPath + "config_comment.txt"), Throws.Nothing);
            Assert.That(Configuration.HeaderSize, Is.EqualTo(19));
        }
        [Test]
        public void Test_ConfigInLineComment()
        {
            Configuration.UpdateConfiguration(defaultPath + "config_inline_comment.txt");
            Assert.That(Configuration.HeaderSize, Is.EqualTo(5));
        }
        [Test]
        public void Test_ConfigInLineCommentError()
        {
            bool ret = Configuration.UpdateConfiguration(defaultPath + "config_inline_comment2.txt");
            Assert.That(ret, Is.EqualTo(false));
        }
        [Test]
        public void Test_ConfigParserWrongContext()
        {
            bool ret = Configuration.UpdateConfiguration(defaultPath + "config_wrong.txt");
            Assert.That(ret, Is.EqualTo(false));
        }
    }
}

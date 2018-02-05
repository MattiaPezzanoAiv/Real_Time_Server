using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RealTimeServer.Test
{
    [TestFixture]
    public class Test_Utility
    {
        [Test]
        public void Test_SetBit()
        {
            byte command = 0;
            command = Utility.SetBitOnByte(command, 0, true);
            Assert.That(command, Is.EqualTo(1));
        }
        [Test]
        public void Test_SetBitVal128()
        {
            byte command = 0;
            command = Utility.SetBitOnByte(command, 7, true);
            Assert.That(command, Is.EqualTo(128));
        }
        [Test]
        public void Test_SetBitVal255()
        {
            byte command = 0;
            for (int i = 0; i < 8; i++)
            {
                command = Utility.SetBitOnByte(command, i, true);
            }
            Assert.That(command, Is.EqualTo(255));
        }
        [Test]
        public void Test_SetBitFalse()
        {
            byte command = 255;
            command = Utility.SetBitOnByte(command, 7, false);
            Assert.That(command, Is.EqualTo(127));
        }
        [Test]
        public void Test_SetBitFrom1To0()
        {
            byte command = 255;
            for (int i = 0; i < 8; i++)
            {
                command = Utility.SetBitOnByte(command, i, false);
            }
            Assert.That(command, Is.EqualTo(0));
        }
        [Test]
        public void Test_IsBitSetGreenLight()
        {
            byte command = 255;
            for (int i = 0; i < 8; i++)
            {
                Assert.That(Utility.IsBitSet(command, i), Is.EqualTo(true));
            }
        }
        [Test]
        public void Test_IsBitSetRedLight()
        {
            byte command = 0;
            for (int i = 0; i < 8; i++)
            {
                Assert.That(Utility.IsBitSet(command, i), Is.Not.EqualTo(true));
            }
        }
    }
}

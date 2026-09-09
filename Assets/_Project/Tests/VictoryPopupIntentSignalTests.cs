using Game.Presentation.UI.Common.Popups;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class VictoryPopupIntentSignalTests
    {
        [TestCase(VictoryPopupIntent.Accept)]
        [TestCase(VictoryPopupIntent.DoubleReward)]
        public void Constructor_PreservesIntent(VictoryPopupIntent intent)
        {
            var signal = new VictoryPopupIntentSignal(intent);

            Assert.That(signal.Intent, Is.EqualTo(intent));
        }
    }
}

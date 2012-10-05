using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenIDE.Core.RScripts;

namespace OpenIDE.Tests.RScript
{
    [TestFixture]
    public class ReactiveScriptTests
    {
        [Test]
        public void Should_match_exact() {
            Assert.That(new RScriptMatcher("autotest.net build").Match("autotest.net build"), Is.True);
        }

        [Test]
        public void Should_match_trailing_wildcard() {
            Assert.That(new RScriptMatcher("autotest.net*").Match("autotest.net build"), Is.True);
        }

        [Test]
        public void Should_match_mid_wildcard() {
            Assert.That(new RScriptMatcher("autot*build").Match("autotest.net build"), Is.True);
        }

        [Test]
        public void Should_match_multiple_wildcard() {
            Assert.That(
                new RScriptMatcher("autotest.net build*SeekUence.WebClient.csproj*succeeded*")
                    .Match("autotest.net build \"C:\\Users\\ack\\storage\\src\\Seekuence\\src\\SeekUence.WebClient\\SeekUence.WebClient.csproj\" \"succeeded\""),
                Is.True);
        }
    }
}

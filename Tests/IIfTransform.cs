﻿#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2019 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests {

    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class IIfTransform {

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void IIfTransform1() {

            const string xml = @"
<add name='TestProcess'>
    <entities>
        <add name='TestData'>
            <rows>
                <add Field1='2' Field2='1' Field3='3' Field4='4' Field5='rockstar' Field6='rock' />
                <add Field1='2' Field2='2' Field3='3' Field4='4' Field5='rockstars' Field6='star' />
            </rows>
            <fields>
                <add name='Field1' type='int' />
                <add name='Field2' type='int' />
                <add name='Field3' type='int' />
                <add name='Field4' type='int' />
                <add name='Field5' />
                <add name='Field6' />
            </fields>
            <calculated-fields>
                <add name='Equal' type='int' t='iif(Field1=Field2,Field3,Field4)' />
                <add name='GreaterThan' type='int' t='iif(Field1 > Field2,Field3,Field4)' />
                <add name='StartsWith' type='int' t='iif(Field5 ^= Field6,Field3,Field4)' />
                <add name='Contains' type='int' t='iif(Field5*=Field6,Field3,Field4)' />
            </calculated-fields>
        </add>
    </entities>
</add>";

            var composer = new CompositionRoot();
            var controller = composer.Compose(xml);

            var output = controller.Read().ToArray();
            var cf = composer.Process.Entities.First().CalculatedFields;

            Assert.AreEqual(4, output[0][cf.First(f=>f.Name=="Equal")], "Should be 4 because Field1 and Field2 are not equal.");
            Assert.AreEqual(3, output[1][cf.First(f => f.Name == "Equal")], "Should be 3 because Field1 and Field2 are equal.");
            Assert.AreEqual(3, output[0][cf.First(f => f.Name == "GreaterThan")], "Should be 3 because Field1 is greater than Field2.");
            Assert.AreEqual(4, output[1][cf.First(f => f.Name == "GreaterThan")], "Should be 4 because Field1 is NOT greater than Field2.");

            Assert.AreEqual(3, output[0][cf.First(f => f.Name == "StartsWith")], "Should be 3 because Field5 starts with Field6.");
            Assert.AreEqual(4, output[1][cf.First(f => f.Name == "StartsWith")], "Should be 4 because Field5 does not start with Field6.");

        }

    }
}

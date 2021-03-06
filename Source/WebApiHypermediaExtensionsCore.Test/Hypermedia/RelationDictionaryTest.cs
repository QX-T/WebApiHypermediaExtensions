﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace WebApiHypermediaExtensionsCore.Test.Hypermedia
{
    [TestClass]
    public class RelationDictionaryTest
    {
        private RelationDictionary relationDictionary;

        [TestInitialize]
        public void TestInit()
        {
            relationDictionary = new RelationDictionary();
        }

        [TestMethod]
        public void OverridesSameRelation()
        {
            var rel1 = new List<string> {"Abc", "Def"};
            var rel2 = new List<string> { "Def", "Abc" };
            var relatedEntity1 = new RelatedEntity(rel1, new HypermediaObjectReference(new TestHypermediaObject()));
            var relatedEntity2 = new RelatedEntity(rel2, new HypermediaObjectReference(new TestHypermediaObject()));

            relationDictionary.Add(relatedEntity1);
            relationDictionary.Add(relatedEntity2);

            Assert.AreEqual(1, relationDictionary.Count);
            Assert.AreEqual(relationDictionary[rel1], relatedEntity2);
        }


        public class TestHypermediaObject : HypermediaObject
        {
        }

    }
}

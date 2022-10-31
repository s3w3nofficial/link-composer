using Alza.LinkComposer.Attributes;
using System;
using System.Collections.Generic;

namespace Alza.LinkComposer.AspNet.Tests
{
    [LinkComposerProjectInfo("Alza.LinkComposer.AspNetCore.Sample")]
    public class HomePageControllerLink : LinkComposerController
    {
        [LinkComposerRoute("navigation", "")]
        internal void Navigation(string test = "ahoj")
        {
        }

        [LinkComposerRoute("get/{id}", "")]
        internal void Get(int? id)
        {
        }

        [LinkComposerRoute("getArray/{ids}", "")]
        internal void GetArray(string[] ids)
        {
        }

        [LinkComposerRoute("getByGuid/{id}", "")]
        internal void GetByGUID(Guid id)
        {
        }

        public class TestQueryModel
        {
            public Int32? Limit { get; set; }

            public System.Int32 Offset { get; set; }

            public IEnumerable<System.Int32> Tests { get; set; }

            public IEnumerable<SubTestQueryModel> SubTests { get; set; }

            public TestEnumQueryModel TestEnumQueryModel { get; set; }
        }

        public class SubTestQueryModel
        {
            public IEnumerable<System.Int32> TestsNullable { get; set; }
        }

        public enum TestEnumQueryModel
        {
            a,
            b
        }

        [LinkComposerRoute("getModel/{id}", "")]
        internal void GetModel(string id, TestQueryModel testQueryModel)
        {
        }

        [LinkComposerRoute("getModel2/{id}", "")]
        internal void GetModel2(string id, TestQueryModel testQueryModel)
        {
        }

        [LinkComposerRoute("getModel3/{id}", "")]
        internal void GetModel3(string id, TestQueryModel[] testQueryModel)
        {
        }

        [LinkComposerRoute("getModel4/{id}", "")]
        internal void GetModel4(string id, string[] names)
        {
        }

        [LinkComposerRoute("postBody", "")]
        internal void PostBody(TestQueryModel testQueryModel)
        {
        }

        [LinkComposerRoute("postPath/{path}", "")]
        internal void PostPath(Uri path)
        {
        }

        [LinkComposerRoute("sameParams/{name}", "")]
        internal void SameParams(string name)
        {
        }

        [LinkComposerRoute("sameParams/{token}", "")]
        internal void SameParams2(string token)
        {
        }

        [LinkComposerRoute("routeParams", "")]
        internal void RouteParams(TestQueryModel model)
        {
        }

        [LinkComposerRoute("Enums", "")]
        internal void Enums(TestEnumQueryModel? model)
        {
        }
    }
}

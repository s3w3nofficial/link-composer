using Alza.LinkComposer.Attributes;
using System;
using System.Collections.Generic;

namespace Alza.LinkComposer.AspNetCore.Tests;

[LinkComposerProjectInfo("Alza.LinkComposer.AspNetCore.Sample", 1)]
public sealed class HomePageControllerLink : LinkComposerController
{
    [LinkComposerRoute("navigation", "api/homepage/v{version:apiVersion}")]
    public void Navigation(string test = "ahoj")
    {
    }

    [LinkComposerRoute("get/{id}", "api/homepage/v{version:apiVersion}")]
    public void Get(int? id)
    {
    }

    [LinkComposerRoute("getArray/{ids}", "api/homepage/v{version:apiVersion}")]
    public void GetArray(string[] ids)
    {
    }

    [LinkComposerRoute("getByGuid/{id}", "api/homepage/v{version:apiVersion}")]
    public void GetByGUID(Guid id)
    {
    }

    [LinkComposerRoute("getModel/{id}", "api/homepage/v{version:apiVersion}")]
    public void GetModel(string id, TestQueryModel testQueryModel)
    {
    }

    [LinkComposerRoute("getModel2/{id}", "api/homepage/v{version:apiVersion}")]
    public void GetModel2(string id, TestQueryModel testQueryModel)
    {
    }

    [LinkComposerRoute("getModel3/{id}", "api/homepage/v{version:apiVersion}")]
    public void GetModel3(string id, TestQueryModel[] testQueryModel)
    {
    }

    [LinkComposerRoute("getModel4/{id}", "api/homepage/v{version:apiVersion}")]
    public void GetModel4(string id, string[] names)
    {
    }

    [LinkComposerRoute("postBody", "api/homepage/v{version:apiVersion}")]
    public void PostBody(TestQueryModel testQueryModel)
    {
    }

    [LinkComposerRoute("postPath/{path}", "api/homepage/v{version:apiVersion}")]
    public void PostPath(Uri path)
    {
    }

    [LinkComposerRoute("sameParams/{name}", "api/homepage/v{version:apiVersion}")]
    public void SameParams(string name)
    {
    }

    [LinkComposerRoute("sameParams/{token}", "api/homepage/v{version:apiVersion}")]
    public void SameParams1(string token)
    {
    }

    [LinkComposerRoute("routeParams", "api/homepage/v{version:apiVersion}")]
    public void RouteParams(TestQueryModel model)
    {
    }

    [LinkComposerRoute("Enums", "api/homepage/v{version:apiVersion}")]
    public void Enums(TestEnumQueryModel? model)
    {
    }

    [LinkComposerRoute("Enums", "api/homepage/v{version:apiVersion}")]
    public void Enums2(TestEnum? model)
    {
    }

    public enum TestEnumQueryModel
    {
        a,
        b
    }

    public enum TestEnum
    {
        A,
        B
    }

    public class TestQueryModel
    {
        public Int32? Limit { get; set; }

        public System.Int32 Offset { get; set; }

        public IEnumerable<System.Int32> Tests { get; set; }

        public IEnumerable<SubTestQueryModel>? SubTests { get; set; }

        public TestEnumQueryModel TestEnumQueryModel { get; set; }

        public System.Data.CommandType CommandType { get; set; }
    }

    public class SubTestQueryModel
    {
        public IEnumerable<System.Int32>? TestsNullable { get; set; }
    }
}

using System;
using NUnit.Framework;
using ConsoleApp.Files;
using Moq;
using System.Collections.Generic;

[TestFixture]
class CustomEntityTest
{
    private CustomEntity _customEntity;
    [SetUp]
    public void SetUp()
    {
        _customEntity = new CustomEntity();
    }
}
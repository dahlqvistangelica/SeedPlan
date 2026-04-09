using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SeedPlan.Client.Components.Functions;
using SeedPlan.Client.Components.Modals;
using SeedPlan.Shared.Interfaces;
using SeedPlan.Shared.Models;

namespace SeedPlan.Client.UnitTests.Flows;

[TestClass]
public class Prio1UiBehaviorTests
{
    [TestMethod]
    public async Task AddDahliaModal_OnSearchInput_WithTwoOrMoreChars_PerformsAsyncSearchAndStoresResults()
    {
        var dahliaService = new Mock<IDahliaService>();
        var expected = new List<Dahlia>
        {
            new() { Id = "d-1", Name = "Cafe au Lait" }
        };

        dahliaService
            .Setup(s => s.SearchDahliasAsync("cafe"))
            .ReturnsAsync(expected);

        var component = new AddDahliaModal
        {
        };
        SetInjectMember(component, "DahliaService", dahliaService.Object);

        await InvokePrivateAsync(component, "OnSearchInput", new ChangeEventArgs { Value = "cafe" });

        var searchResults = GetPrivateField<List<Dahlia>>(component, "searchResults");
        var isSearching = GetPrivateField<bool>(component, "isSearching");

        Assert.AreEqual(1, searchResults.Count);
        Assert.AreEqual("Cafe au Lait", searchResults[0].Name);
        Assert.IsFalse(isSearching);
        dahliaService.Verify(s => s.SearchDahliasAsync("cafe"), Times.Once);
    }

    [TestMethod]
    public async Task AddDahliaModal_OnSearchInput_WithShortTerm_ClearsResultsAndSkipsSearch()
    {
        var dahliaService = new Mock<IDahliaService>();
        var component = new AddDahliaModal
        {
        };
        SetInjectMember(component, "DahliaService", dahliaService.Object);

        await InvokePrivateAsync(component, "OnSearchInput", new ChangeEventArgs { Value = "a" });

        var searchResults = GetPrivateField<List<Dahlia>>(component, "searchResults");
        var isSearching = GetPrivateField<bool>(component, "isSearching");

        Assert.AreEqual(0, searchResults.Count);
        Assert.IsFalse(isSearching);
        dahliaService.Verify(s => s.SearchDahliasAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task AddDahliaModal_SelectResult_UpdatesSelectionAndClearsSearchState()
    {
        var component = new AddDahliaModal();
        var selected = new Dahlia { Id = "d-2", Name = "Wizard of Oz" };

        SetPrivateField(component, "searchText", "wizard");
        SetPrivateField(component, "searchResults", new List<Dahlia> { selected });

        InvokePrivate(component, "SelectResult", selected);

        var isSelected = GetPrivateField<bool>(component, "isDahliaSelected");
        var searchText = GetPrivateField<string>(component, "searchText");
        var searchResults = GetPrivateField<List<Dahlia>>(component, "searchResults");
        var newDahlia = GetPrivateField<UserDahlia>(component, "newDahlia");

        Assert.IsTrue(isSelected);
        Assert.AreEqual(string.Empty, searchText);
        Assert.AreEqual(0, searchResults.Count);
        Assert.AreEqual("d-2", newDahlia.VarietyId);
        Assert.IsNotNull(newDahlia.Variety);
        Assert.AreEqual("Wizard of Oz", newDahlia.Variety.Name);
    }

    [TestMethod]
    public async Task StarRating_WhenClickingSameStar_TogglesToNull()
    {
        var component = new StarRating { Rating = 3 };
        int? callbackValue = -1;
        component.RatingChanged = EventCallback.Factory.Create<int?>(this, v => callbackValue = v);

        await InvokePrivateAsync(component, "HandleClick", 3);

        Assert.IsNull(callbackValue);
    }

    [TestMethod]
    public async Task StarRating_WhenClickingDifferentStar_EmitsNewValue()
    {
        var component = new StarRating { Rating = 2 };
        int? callbackValue = null;
        component.RatingChanged = EventCallback.Factory.Create<int?>(this, v => callbackValue = v);

        await InvokePrivateAsync(component, "HandleClick", 5);

        Assert.AreEqual(5, callbackValue);
    }

    private static void InvokePrivate(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method, $"Method '{methodName}' was not found on {target.GetType().Name}.");
        method.Invoke(target, args);
    }

    private static async Task InvokePrivateAsync(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method, $"Method '{methodName}' was not found on {target.GetType().Name}.");

        var result = method.Invoke(target, args);
        if (result is Task task)
        {
            await task;
        }
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, $"Field '{fieldName}' was not found on {target.GetType().Name}.");
        return (T)field.GetValue(target)!;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, $"Field '{fieldName}' was not found on {target.GetType().Name}.");
        field.SetValue(target, value);
    }

    private static void SetInjectMember(object target, string memberName, object value)
    {
        var type = target.GetType();

        var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null)
        {
            property.SetValue(target, value);
            return;
        }

        var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetField(char.ToLowerInvariant(memberName[0]) + memberName[1..], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Assert.IsNotNull(field, $"Inject member '{memberName}' was not found on {type.Name}.");
        field.SetValue(target, value);
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GravityControllerTests
{
    private GameObject testObject;
    private GravityController gravityController;

    [SetUp]
    public void SetUp()
    {
        // Crear GameObject amb GravityController
        testObject = new GameObject("TestObject");
        testObject.AddComponent<Rigidbody>();
        gravityController = testObject.AddComponent<GravityController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(testObject);
    }

    // TEST 1: Verificar estat inicial de la gravetat
    [Test]
    public void GravityStartsNormal()
    {
        Assert.IsFalse(gravityController.IsGravityInverted());
    }

    // TEST 2: Verificar que es Toggle per canviar gravetat funciona
    [Test]
    public void ToggleChangesGravity()
    {
        gravityController.ToggleGravity();
        Assert.IsTrue(gravityController.IsGravityInverted());
        
        gravityController.ToggleGravity();
        Assert.IsFalse(gravityController.IsGravityInverted());
    }

    // TEST 3: Verificar si s'inverteix sa gravetat correctament
    [Test]
    public void SetGravityInvertedWorks()
    {
        gravityController.SetGravityInverted(true);
        Assert.IsTrue(gravityController.IsGravityInverted());
        
        gravityController.SetGravityInverted(false);
        Assert.IsFalse(gravityController.IsGravityInverted());
    }
}

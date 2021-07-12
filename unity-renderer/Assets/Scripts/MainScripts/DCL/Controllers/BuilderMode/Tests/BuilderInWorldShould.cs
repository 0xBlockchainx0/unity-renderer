using Builder;
using Cinemachine;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class BuilderInWorldShould : IntegrationTestSuite_Legacy
{
    [Test]
    public void GroundRaycast()
    {
        //Arrange
        RaycastHit hit;

        Vector3 fromPosition = new Vector3(0, 10, 0);
        Vector3 toPosition = Vector3.zero;
        Vector3 direction = toPosition - fromPosition;

        bool groundLayerFound = Physics.Raycast(fromPosition, direction, out hit, BuilderInWorldGodMode.RAYCAST_MAX_DISTANCE, BIWSettings.GROUND_LAYER);

        Camera.main.transform.LookAt(Vector3.zero);
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        //Act
        if (Physics.Raycast(ray, out hit, BuilderInWorldGodMode.RAYCAST_MAX_DISTANCE, BIWSettings.GROUND_LAYER))
        {
            groundLayerFound = true;
        }

        //Assert
        Assert.IsTrue(groundLayerFound, "The ground layer is not set to Ground");
    }

    [Test]
    public void BuilderInWorldEntityComponents()
    {
        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        DCLBuilderInWorldEntity biwEntity = Utils.GetOrCreateComponent<DCLBuilderInWorldEntity>(scene.entities[entityId].gameObject);
        biwEntity.Init(scene.entities[entityId], null);

        Assert.IsTrue(biwEntity.entityUniqueId == scene.sceneData.id + scene.entities[entityId].entityId, "Entity id is not created correctly, this can lead to weird behaviour");

        SmartItemComponent.Model model = new SmartItemComponent.Model();

        scene.EntityComponentCreateOrUpdateWithModel(entityId, CLASS_ID_COMPONENT.SMART_ITEM, model);

        Assert.IsTrue(biwEntity.HasSmartItemComponent());

        DCLName name = (DCLName) scene.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.NAME));
        scene.SharedComponentAttach(biwEntity.rootEntity.entityId, name.id);

        DCLName dclName = biwEntity.rootEntity.TryGetComponent<DCLName>();
        Assert.IsNotNull(dclName);

        string newName = "TestingName";
        dclName.SetNewName(newName);
        Assert.AreEqual(newName, biwEntity.GetDescriptiveName());


        DCLLockedOnEdit entityLocked = (DCLLockedOnEdit) scene.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.LOCKED_ON_EDIT));
        scene.SharedComponentAttach(biwEntity.rootEntity.entityId, entityLocked.id);

        DCLLockedOnEdit dclLockedOnEdit = biwEntity.rootEntity.TryGetComponent<DCLLockedOnEdit>();
        Assert.IsNotNull(dclLockedOnEdit);

        bool isLocked = true;
        dclLockedOnEdit.SetIsLocked(isLocked);
        Assert.AreEqual(biwEntity.IsLocked, isLocked);
    }

    protected override IEnumerator TearDown()
    {
        AssetCatalogBridge.i.ClearCatalog();
        BIWCatalogManager.ClearCatalog();
        yield return base.TearDown();
    }
}
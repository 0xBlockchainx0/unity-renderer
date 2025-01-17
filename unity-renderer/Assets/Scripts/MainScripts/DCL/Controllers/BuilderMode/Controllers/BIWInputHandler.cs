using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BIWInputHandler : BIWController
{
    [Header("Design variables")]
    public float msBetweenInputInteraction = 200;

    [Header("References")]
    public BuilderInWorldController builderInWorldController;
    public ActionController actionController;
    public BIWModeController biwModeController;
    public BuilderInWorldInputWrapper builderInputWrapper;
    public BIWOutlinerController outlinerController;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;

    [Header("InputActions")]
    [SerializeField]
    internal InputAction_Trigger editModeChangeInputAction;

    [SerializeField]
    internal InputAction_Trigger toggleRedoActionInputAction;

    [SerializeField]
    internal InputAction_Trigger toggleUndoActionInputAction;

    [SerializeField]
    internal InputAction_Hold multiSelectionInputAction;

    private InputAction_Hold.Started multiSelectionStartDelegate;
    private InputAction_Hold.Finished multiSelectionFinishedDelegate;

    private InputAction_Trigger.Triggered redoDelegate;
    private InputAction_Trigger.Triggered undoDelegate;

    private bool isMultiSelectionActive = false;

    private float nexTimeToReceiveInput;

    void Start()
    {
        editModeChangeInputAction.OnTriggered += OnEditModeChangeAction;

        redoDelegate = (action) => RedoAction();
        undoDelegate = (action) => UndoAction();

        toggleRedoActionInputAction.OnTriggered += redoDelegate;
        toggleUndoActionInputAction.OnTriggered += undoDelegate;

        multiSelectionStartDelegate = (action) => StartMultiSelection();
        multiSelectionFinishedDelegate = (action) => EndMultiSelection();

        BuilderInWorldInputWrapper.OnMouseClick += MouseClick;
        BuilderInWorldInputWrapper.OnMouseClickOnUI += MouseClickOnUI;
        biwModeController.OnInputDone += InputDone;

        multiSelectionInputAction.OnStarted += multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished += multiSelectionFinishedDelegate;
    }

    private void OnDestroy()
    {
        editModeChangeInputAction.OnTriggered -= OnEditModeChangeAction;

        toggleRedoActionInputAction.OnTriggered -= redoDelegate;
        toggleUndoActionInputAction.OnTriggered -= undoDelegate;

        multiSelectionInputAction.OnStarted -= multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished -= multiSelectionFinishedDelegate;

        BuilderInWorldInputWrapper.OnMouseClick -= MouseClick;
        BuilderInWorldInputWrapper.OnMouseClickOnUI -= MouseClickOnUI;
        biwModeController.OnInputDone -= InputDone;
        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnStopInput -= StopInput;
            HUDController.i.builderInWorldMainHud.OnResumeInput -= ResumeInput;
        }
    }

    public override void Init()
    {
        base.Init();
        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnStopInput += StopInput;
            HUDController.i.builderInWorldMainHud.OnResumeInput += ResumeInput;
        }
    }

    protected override void FrameUpdate()
    {
        base.FrameUpdate();

        if (Time.timeSinceLevelLoad < nexTimeToReceiveInput)
            return;

        if (Utils.isCursorLocked || biwModeController.IsGodModeActive())
            CheckEditModeInput();
        biwModeController.CheckInput();
    }

    public override void EnterEditMode(ParcelScene scene)
    {
        base.EnterEditMode(scene);
        builderInputWrapper.gameObject.SetActive(true);
    }

    public override void ExitEditMode()
    {
        base.ExitEditMode();
        builderInputWrapper.gameObject.SetActive(false);
    }

    private void CheckEditModeInput()
    {
        outlinerController.CheckOutline();

        if (builderInWorldEntityHandler.IsAnyEntitySelected())
        {
            biwModeController.CheckInputSelectedEntities();
        }
    }

    private void StartMultiSelection()
    {
        isMultiSelectionActive = true;
        builderInWorldEntityHandler.SetMultiSelectionActive(isMultiSelectionActive);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        biwModeController.StartMultiSelection();
    }

    private void EndMultiSelection()
    {
        isMultiSelectionActive = false;
        builderInWorldEntityHandler.SetMultiSelectionActive(isMultiSelectionActive);
        biwModeController.EndMultiSelection();
        outlinerController.CancelUnselectedOutlines();
    }

    private void MouseClick(int buttonID, Vector3 position)
    {
        if (!isEditModeActive)
            return;

        if (Time.timeSinceLevelLoad < nexTimeToReceiveInput)
            return;

        if (!BuilderInWorldUtils.IsPointerOverUIElement())
            HUDController.i.builderInWorldMainHud.HideExtraBtns();

        if (Utils.isCursorLocked || biwModeController.IsGodModeActive())
        {
            if (buttonID == 0)
            {
                MouseClickDetected();
                InputDone();
                return;
            }
            outlinerController.CheckOutline();
        }
    }

    private void MouseClickOnUI(int buttonID, Vector3 position) { HUDController.i.builderInWorldMainHud.HideExtraBtns(); }

    public bool IsMultiSelectionActive() => isMultiSelectionActive;

    private void OnEditModeChangeAction(DCLAction_Trigger action) { builderInWorldController.ChangeEditModeStatusByShortcut(); }

    private void RedoAction()
    {
        actionController.TryToRedoAction();
        InputDone();
    }

    private void UndoAction()
    {
        InputDone();

        if (biwModeController.ShouldCancelUndoAction())
            return;

        actionController.TryToUndoAction();
    }

    private void MouseClickDetected() { biwModeController.MouseClickDetected(); }

    private void InputDone() { nexTimeToReceiveInput = Time.timeSinceLevelLoad + msBetweenInputInteraction / 1000; }

    private void StopInput() { builderInputWrapper.StopInput(); }

    private void ResumeInput() { builderInputWrapper.ResumeInput(); }
}
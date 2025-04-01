using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;

public class Player : NetworkBehaviour
{
    [SerializeField] private MeshRenderer[] modelParts;
    [SerializeField] private SimpleKCC kcc;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpImpulse = 10f;
    [SerializeField] private Transform camTarget;
    [SerializeField] private float lookSensitivity = 0.15f;

    [SerializeField] private Canvas itCanvas;

    [Networked] private NetworkButtons PreviousButtons { get; set; }
    [Networked] public bool IsIt { get; set; }


    public override void Spawned()
    {
        kcc.SetGravity(Physics.gravity.y * 2f);

        if (HasInputAuthority)
        {
            foreach (MeshRenderer renderer in modelParts)
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

            CameraFollow.Singleton.SetTarget(camTarget);
        }

        if (itCanvas != null)
            itCanvas.gameObject.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput input))
        {
            kcc.AddLookRotation(input.LookDelta * lookSensitivity);
            UpdateCamTarget();
            Vector3 worldDirection = kcc.TransformRotation * new Vector3(input.Direction.x, 0f, input.Direction.y);
            float jump = 0f;

            if (input.Buttons.WasPressed(PreviousButtons, InputButton.Jump) && kcc.IsGrounded)
                jump = jumpImpulse;

            kcc.Move(worldDirection.normalized * speed, jump);
            PreviousButtons = input.Buttons;
        }
    }

    public override void Render()
    {
        UpdateCamTarget();

         if (itCanvas != null)
            itCanvas.gameObject.SetActive(HasInputAuthority && IsIt);
    }

    private void UpdateCamTarget()
    {
        camTarget.localRotation = Quaternion.Euler(kcc.GetLookRotation().x, 0f, 0f);
    }
}

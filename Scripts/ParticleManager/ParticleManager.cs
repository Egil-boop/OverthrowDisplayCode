using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class ParticleManager : NetworkBehaviour
{
    [SerializeField] private GameObject burnMark;
    [SerializeField] private GameObject imGameObj;

    [SerializeField] private GameObject grenadeExplosionEffect;

    [SerializeField] private VisualEffect dustParticles;

    [SerializeField] private ParticleSystem movementParticleSystem;
    [SerializeField] private ParticleSystem shootPariParticleSystem;
    [SerializeField] private ParticleSystem impactPart;

    [SerializeField] private ParticleSystem speedBoostParticleSystem;

    [SerializeField] private ParticleSystem meleeImpactParticleSystem;


    [SerializeField] private OwnerNetworkAnimator OwnerAnimatorForBazooka;
    [SerializeField] private OwnerNetworkAnimator ownerNetworkAnimatorForGrenadeLauncher;

    [SerializeField] private Transform localPlayerPos;

    private Transform particleSystemTransform;
    private PlayerWorldInformation playerWorldInformation;
    

    private void Awake()
    {
        particleSystemTransform = movementParticleSystem.transform;
    }

    private void Start()
    {
        playerWorldInformation =
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerWorldInformation>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayGrenadeExplosionServerRpc(Vector3 explosionPoint)
    {
        PlayGrenadeExplosionClientRpc(explosionPoint);
    }

    [ClientRpc]
    private void PlayGrenadeExplosionClientRpc(Vector3 explosionPoint)
    {
        foreach (var player in playerWorldInformation.playersInGame)
        {
            player.GetComponent<ParticleManager>().PlayGrenadeExplosion(explosionPoint);
        }
    }

    private async void PlayGrenadeExplosion(Vector3 explosionPoint)
    {
        GameObject rocketExplosion = Instantiate(grenadeExplosionEffect, explosionPoint, Quaternion.identity);
        float explosionEffectCountdown = rocketExplosion.GetComponent<ParticleSystem>().main.duration;

        while (explosionEffectCountdown > 0)
        {
            explosionEffectCountdown -= Time.deltaTime;
            await Task.Yield();
        }

        Destroy(rocketExplosion);
    }

    public void PlayGrenadeAnimation()
    {
        ownerNetworkAnimatorForGrenadeLauncher.SetTrigger("Shoot");
    }

    public void PlayBazookaAnimation()
    {
        OwnerAnimatorForBazooka.SetTrigger("Shooting");
    }

    public void PlayMovementParticle(Vector3 pos, Vector3 groundHitNormal)
    {
        particleSystemTransform.position = pos;
        particleSystemTransform.rotation = Quaternion.FromToRotation(Vector3.forward, groundHitNormal);
        movementParticleSystem.Play();
        dustParticles.Play();
    }

    private void StopMovementParticleSystem()
    {
        movementParticleSystem.Stop();
        dustParticles.Stop();
    }

    private void PlayKnockBackLocal()
    {
        shootPariParticleSystem.Play();
    }

    private void PlayMeleeImpactParticle()
    {
        meleeImpactParticleSystem.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayMeleeImpactParticleServerRpc(ulong whoGotHit)
    {
        PlayMeleeImpactParticleClientRpc(whoGotHit);
    }

    [ClientRpc]
    private void PlayMeleeImpactParticleClientRpc(ulong whoGotHit)
    {
        foreach (var variable in playerWorldInformation.playersInGame)
        {
            if (variable.GetComponent<NetworkObject>().OwnerClientId == whoGotHit)
            {
                variable.GetComponent<ParticleManager>().PlayMeleeImpactParticle();
            }
        }
    }

    private void StopPlaySpeedBoostParticle()
    {
        speedBoostParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        speedBoostParticleSystem.gameObject.SetActive(false);
    }


    [ServerRpc(RequireOwnership = false)]
    public void StopPlaySpeedBoostParticleServerRpc(ServerRpcParams serverRpcParams = default)
    {
        StopPlaySpeedBoostParticleClientRpc(serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void StopPlaySpeedBoostParticleClientRpc(ulong playerWhoSent)
    {
        foreach (var variable in playerWorldInformation.playersInGame)
        {
            if (variable.GetComponent<NetworkObject>().OwnerClientId == playerWhoSent)
            {
                variable.GetComponent<ParticleManager>().StopPlaySpeedBoostParticle();
            }
        }
    }
    private void PlaySpeedBoostParticle()
    {
        if (speedBoostParticleSystem.gameObject.activeSelf == false)
        {
            speedBoostParticleSystem.gameObject.SetActive(true);
        }
        speedBoostParticleSystem.Play();
    }


    [ServerRpc(RequireOwnership = false)]
    public void PlaySpeedBoostParticleServerRpc(ServerRpcParams serverRpcParams = default)
    {
        PlaySpeedBoostParticleClientRpc(serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void PlaySpeedBoostParticleClientRpc(ulong playerWhoSent)
    {
        foreach (var variable in playerWorldInformation.playersInGame)
        {
            if (variable.GetComponent<NetworkObject>().OwnerClientId == playerWhoSent)
            {
                variable.GetComponent<ParticleManager>().PlaySpeedBoostParticle();
            }
        }
    }

    private void PlayImpactParticle(Vector3 pos)
    {
        var relPoint = pos - localPlayerPos.position;

        var x = Vector3.Dot(relPoint, localPlayerPos.transform.right);
        var y = Vector3.Dot(relPoint, localPlayerPos.transform.up);
        var z = Vector3.Dot(relPoint, localPlayerPos.transform.forward);

        var space = new Vector3(x, y, z);

        imGameObj.transform.localPosition = space;

        var burnMarkInstance = Instantiate(burnMark, localPlayerPos);
        burnMarkInstance.transform.localPosition = space;

        impactPart.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayImpactParticlesServerRpc(Vector3 hitPos, ulong hitPlayer)
    {
        PlayImpactParticlesClientRpc(hitPos, hitPlayer);
    }

    [ClientRpc]
    private void PlayImpactParticlesClientRpc(Vector3 hitPos, ulong playerWhoGotHit)
    {
        foreach (var variable in playerWorldInformation.playersInGame)
        {
            if (variable.GetComponent<NetworkObject>().OwnerClientId == playerWhoGotHit)
            {
                variable.GetComponent<ParticleManager>().PlayImpactParticle(hitPos);
            }
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void PlayKnockBackServerRpc(ServerRpcParams serverRpcParams = default)
    {
        PlayKnockBackParticlesClientRpc(serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void PlayKnockBackParticlesClientRpc(ulong playerWhoSent)
    {
        foreach (var variable in playerWorldInformation.playersInGame)
        {
            if (variable.GetComponent<NetworkObject>().OwnerClientId == playerWhoSent)
            {
                variable.GetComponent<ParticleManager>().PlayKnockBackLocal();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayMyParticlesServerRpc(Vector3 pos, Vector3 groundHitNormal,
        ServerRpcParams serverRpcParams = default)
    {
        PlayMyParticlesClientRpc(serverRpcParams.Receive.SenderClientId, pos, groundHitNormal);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopPlayingParticleSystemServerRpc(ServerRpcParams serverRpcParams = default)
    {
        StopMyParticlesClientRpc(serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void PlayMyParticlesClientRpc(ulong playerWhoSent, Vector3 pos, Vector3 groundHitNormal)
    {
        foreach (var variable in playerWorldInformation.playersInGame)
        {
            
            if (variable.GetComponent<NetworkObject>().OwnerClientId == playerWhoSent)
            {
                variable.GetComponent<ParticleManager>().PlayMovementParticle(pos, groundHitNormal);
            }
        }
        
        
    }

    [ClientRpc]
    private void StopMyParticlesClientRpc(ulong playerWhoSent)
    {
        foreach (var variable in playerWorldInformation.playersInGame)
        {
            if (variable.GetComponent<NetworkObject>().OwnerClientId == playerWhoSent)
            {
                variable.GetComponent<ParticleManager>().StopMovementParticleSystem();
            }
        }
    }
}
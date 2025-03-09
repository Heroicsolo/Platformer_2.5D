using HeroicEngine.Components.Combat;
using HeroicEngine.Systems.DI;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.Gameplay
{
    public interface IHittablesManager : ISystem
    {
        void RegisterHittable(Hittable hittable);
        void UnregisterHittable(Hittable hittable);
        List<Hittable> GetHittablesInRadius(Vector3 from, float radius);
        List<Hittable> GetTeamHittablesInRadius(Vector3 from, float radius, TeamType teamType);
        List<Hittable> GetOtherTeamsHittablesInRadius(Vector3 from, float radius, TeamType excludedTeam);
        void KillTeam(TeamType teamType);
    }
}
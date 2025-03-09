using HeroicEngine.Components.Combat;
using HeroicEngine.Systems.DI;
using HeroicEngine.Utils.Math;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Systems.Gameplay
{
    public class HittablesManager : SystemBase, IHittablesManager
    {
        private Dictionary<TeamType, List<Hittable>> teamsHittables = new Dictionary<TeamType, List<Hittable>>();
        private List<Hittable> allHittables = new List<Hittable>();

        /// <summary>
        /// This method registers hittable object in manager. Other characters will be able to find it.
        /// </summary>
        /// <param name="hittable">Hittable object</param>
        public void RegisterHittable(Hittable hittable)
        {
            if (!teamsHittables.ContainsKey(hittable.TeamType))
            {
                teamsHittables.Add(hittable.TeamType, new List<Hittable>{ hittable });
            }
            else if (!teamsHittables[hittable.TeamType].Contains(hittable))
            {
                teamsHittables[hittable.TeamType].Add(hittable);
            }

            if (!allHittables.Contains(hittable))
            {
                allHittables.Add(hittable);
            }
        }

        /// <summary>
        /// This method unregisters hittable object from manager. Other characters and projectiles will not be able to find it.
        /// </summary>
        /// <param name="hittable">Hittable object</param>
        public void UnregisterHittable(Hittable hittable)
        {
            if (teamsHittables[hittable.TeamType].Contains(hittable))
            {
                teamsHittables[hittable.TeamType].Remove(hittable);
            }
            if (allHittables.Contains(hittable))
            {
                allHittables.Remove(hittable);
            }
        }

        /// <summary>
        /// This method returns all Hittable objects in certain radius from given point.
        /// </summary>
        /// <param name="from">Given point</param>
        /// <param name="radius">Search radius</param>
        /// <returns>List of found Hittable objects</returns>
        public List<Hittable> GetHittablesInRadius(Vector3 from, float radius)
        {
            if (allHittables.Count == 0)
            {
                return new List<Hittable>();
            }

            List<Hittable> selectedHittables = new List<Hittable>();

            foreach (Hittable hittable in allHittables)
            {
                if (hittable == null || hittable.IsDead())
                {
                    continue;
                }
                if (hittable.transform.position.Distance(from) <= radius)
                {
                    selectedHittables.Add(hittable);
                }
            }

            return selectedHittables;
        }

        /// <summary>
        /// This method returns all Hittable objects in certain radius from given point, from all teams except excludedTeam.
        /// </summary>
        /// <param name="from">Given point</param>
        /// <param name="radius">Search radius</param>
        /// <param name="excludedTeam">Excluded team</param>
        /// <returns>List of found Hittable objects</returns>
        public List<Hittable> GetOtherTeamsHittablesInRadius(Vector3 from, float radius, TeamType excludedTeam)
        {
            List<Hittable> result = new List<Hittable>();
            
            foreach (TeamType teamType in teamsHittables.Keys)
            {
                if (teamType != excludedTeam)
                {
                    result.AddRange(GetTeamHittablesInRadius(from, radius, teamType));
                }
            }

            return result;
        }

        /// <summary>
        /// This method returns all Hittable objects in certain radius from given point, from certain team.
        /// </summary>
        /// <param name="from">Given point</param>
        /// <param name="radius">Search radius</param>
        /// <param name="teamType">Hittables team</param>
        /// <returns>List of found Hittable objects</returns>
        public List<Hittable> GetTeamHittablesInRadius(Vector3 from, float radius, TeamType teamType)
        {
            if (!teamsHittables.ContainsKey(teamType))
            {
                return new List<Hittable>();
            }

            List<Hittable> teamHittables = teamsHittables[teamType];

            if (teamHittables.Count == 0)
            {
                return teamHittables;
            }

            List<Hittable> selectedHittables = new List<Hittable>();

            foreach (Hittable hittable in teamHittables)
            {
                if (hittable == null || hittable.IsDead())
                {
                    continue;
                }
                if (hittable.transform.position.Distance(from) <= radius)
                {
                    selectedHittables.Add(hittable);
                }
            }

            return selectedHittables;
        }

        /// <summary>
        /// This method instantly kills all Hittable objects of certain team.
        /// </summary>
        /// <param name="teamType">Team</param>
        public void KillTeam(TeamType teamType)
        {
            if (teamsHittables.ContainsKey(teamType))
            {
                teamsHittables[teamType].ForEach(hittable => hittable.Kill());
            }
        }
    }
}
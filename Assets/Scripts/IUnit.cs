using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnit
{
    void TakeDamage(float amount);
    IEnumerator PerformAttack();

    void PerformMove();

    IEnumerator PerformDeath(float waitTime);

    void PerformVictory();

    void GoTo(List<EnvironmentTile> route);
    
}

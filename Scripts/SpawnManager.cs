using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum PlayerTeam
{
    None,
    BlueTeam,
    RedTeam
}

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> _sharedSpawnPoints = new List<SpawnPoint>();
    System.Random _random = new System.Random();
	float _closestDistance = Mathf.Infinity;
    [Tooltip("This will be used to calculate the second filter where algorithm looks for closest friends, if the friends are away from this value, they will be ignored")]
    [SerializeField] private float _maxDistanceToClosestFriend = 30;
    [Tooltip("This will be used to calculate the first filter where algorithm looks for enemies that are far away from this value. Only enemies which are away from this value will be calculated.")]
    [SerializeField] private float _minDistanceToClosestEnemy = 10;
    [Tooltip("This value is to prevent friendly player spawning on top of eachothers. If a player is within the range of this value to a spawn point, that spawn point will be ignored")]
    [SerializeField] private float _minMemberDistance = 2;

	// Editor icersinde assign edilecek
    public DummyPlayer PlayerToBeSpawned;

	// Bu script awake oldugunda bolum icersinden DummyPlayer'lari bulcak
    public DummyPlayer[] DummyPlayers;

    private void Awake()
    {
		_sharedSpawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());

		DummyPlayers = FindObjectsOfType<DummyPlayer>();
    }

    #region SPAWN ALGORITHM
    public SpawnPoint GetSharedSpawnPoint(PlayerTeam team)
    {
        List<SpawnPoint> spawnPoints = new List<SpawnPoint>(_sharedSpawnPoints.Count);
        CalculateDistancesForSpawnPoints(team);
        GetSpawnPointsByDistanceSpawning(team, ref spawnPoints);
        if (spawnPoints.Count <= 0)
        {
			GetSpawnPointsBySquadSpawning(team, ref spawnPoints);
        }		
        SpawnPoint spawnPoint = spawnPoints.Count <= 1 ? spawnPoints[0] : spawnPoints[_random.Next(0, (int)((float)spawnPoints.Count * .5f))];
        spawnPoint.StartTimer();
        return spawnPoint;
    }

    private void GetSpawnPointsByDistanceSpawning(PlayerTeam team, ref List<SpawnPoint> suitableSpawnPoints)
    {
		//Please apply your algorithm here


		// bir nedenden dolayi listemiz metoda arguman olarak null yollanmis ise bir liste olusturuyoruz, null gelmediyse de listemizi temizliyoruz
		if (suitableSpawnPoints == null)
		{
			suitableSpawnPoints = new List<SpawnPoint>();
		}
		suitableSpawnPoints.Clear();

		//spawn pointlerimizin oldugu listeyi ilk once en yakin dusmana olan uzakliga gore sortluyoruz
		_sharedSpawnPoints.Sort(delegate (SpawnPoint a, SpawnPoint b)
		{
			if (a.DistanceToClosestEnemy == b.DistanceToClosestEnemy)
			{
				return 0;
			}
			if (a.DistanceToClosestEnemy > b.DistanceToClosestEnemy)
			{
				return 1;
			}
			return -1;
		});

		// simdi tek tek butun spawn pointlerde looplayip parametrelerimize uygun olup olmadigi incelenecek
		for (int i = 0; i < _sharedSpawnPoints.Count; i++)
		{
			// eger parametrelere uygun bir spawn point bulunursa suitableSpawnPoints listesine eklenecek
			if (!(_sharedSpawnPoints[i].DistanceToClosestFriend <= _minMemberDistance) && _sharedSpawnPoints[i].DistanceToClosestEnemy >= _minDistanceToClosestEnemy && !(_sharedSpawnPoints[i].DistanceToClosestEnemy <= _minMemberDistance) && _sharedSpawnPoints[i].SpawnTimer <= 0)
			{
				Debug.Log(_sharedSpawnPoints[i].gameObject.name + " parametrelere uygun oldugu icin listeye eklendi!");
				suitableSpawnPoints.Add(_sharedSpawnPoints[i]);
				break;
			}
			// parametrelere uygun olmayan spawn pointler icin log olusturuyoruz
			else
			{
				// eger spawn point herhangi bir oyuncuya fazla yakinsa secilmeyecek. Comparison icin kullanilcak olan degisken _minMemberDistance
				if(_sharedSpawnPoints[i].DistanceToClosestFriend <= _minMemberDistance || _sharedSpawnPoints[i].DistanceToClosestEnemy <= _minMemberDistance)
				{
					Debug.Log(_sharedSpawnPoints[i].gameObject.name + " bir oyuncuya cok yakin oldugu icin secilmedi");
				}
				// eger spawn pointin en yakindaki dusmani minimum uzakliktan daha yakinsa secilmeyecek. Comparison icin kullanilcak olan degisken _minDistanceToClosestEnemy
				if(_sharedSpawnPoints[i].DistanceToClosestEnemy < _minDistanceToClosestEnemy)
				{
					Debug.Log(_sharedSpawnPoints[i].gameObject.name + " dusmana olmasi gereken minimum uzakliktan daha yakin oldugu icin secilmedi");
				}
				// spawn pointin timeri dolmadiysa secilmeyecek
				if(!(_sharedSpawnPoints[i].SpawnTimer <= 0))
				{
					Debug.Log(_sharedSpawnPoints[i].gameObject.name + " timeri daha dolmadi o yuzden secilmedi.");
				}
			}
		}		
	}

    private void GetSpawnPointsBySquadSpawning(PlayerTeam team, ref List<SpawnPoint> suitableSpawnPoints)
    {
        if (suitableSpawnPoints == null)
        {
            suitableSpawnPoints = new List<SpawnPoint>();
        }
        suitableSpawnPoints.Clear();
        _sharedSpawnPoints.Sort(delegate (SpawnPoint a, SpawnPoint b)
        {
            if (a.DistanceToClosestFriend == b.DistanceToClosestFriend)
            {
                return 0;
            }
            if (a.DistanceToClosestFriend > b.DistanceToClosestFriend)
            {
                return 1;
            }
            return -1;
        });
		// for loop'un condition kismindaki ifade yuzunden loop butun spawn pointler arasinda donmuyordu,
		// o yuzden o ifade if statement'in icine koyuldu
        for (int i = 0; i < _sharedSpawnPoints.Count; i++) // && _sharedSpawnPoints[i].DistanceToClosestFriend <= _maxDistanceToClosestFriend
		{
            if (!(_sharedSpawnPoints[i].DistanceToClosestFriend <= _minMemberDistance) && _sharedSpawnPoints[i].DistanceToClosestFriend <= _maxDistanceToClosestFriend && !(_sharedSpawnPoints[i].DistanceToClosestEnemy <= _minMemberDistance) && _sharedSpawnPoints[i].SpawnTimer <= 0)
            {
                suitableSpawnPoints.Add(_sharedSpawnPoints[i]);
				break;
            }
        }
        if (suitableSpawnPoints.Count <= 0)
        {
            suitableSpawnPoints.Add(_sharedSpawnPoints[0]);
        }

    }

    private void CalculateDistancesForSpawnPoints(PlayerTeam playerTeam)
    {
        for (int i = 0; i < _sharedSpawnPoints.Count; i++)
        {
            _sharedSpawnPoints[i].DistanceToClosestFriend = GetDistanceToClosestMember(_sharedSpawnPoints[i].PointTransform.position, playerTeam);
            _sharedSpawnPoints[i].DistanceToClosestEnemy = GetDistanceToClosestMember(_sharedSpawnPoints[i].PointTransform.position, playerTeam == PlayerTeam.BlueTeam ? PlayerTeam.RedTeam : playerTeam == PlayerTeam.RedTeam ? PlayerTeam.BlueTeam : PlayerTeam.None);
        }
    }

    private float GetDistanceToClosestMember(Vector3 position, PlayerTeam playerTeam)
    {
		// _closestDistance her seferinde buyuk bir sayiya esitlenmezse bu algoritma calismaz
		_closestDistance = Mathf.Infinity;

        foreach (var player in DummyPlayers)
        {
            if (!player.Disabled && player.PlayerTeamValue != PlayerTeam.None && player.PlayerTeamValue == playerTeam && !player.IsDead())
            {
                float playerDistanceToSpawnPoint = Vector3.Distance(position, player.transform.position);
				//Debug.Log("Spawn point position: " + position + player.gameObject.name + " distance to Spawn point: " + playerDistanceToSpawnPoint + " Team: " + playerTeam);
                if (playerDistanceToSpawnPoint < _closestDistance)
                {
                    _closestDistance = playerDistanceToSpawnPoint;
                }
            }
        }
        return _closestDistance;
    }

    #endregion
	/// <summary>
	/// Test için paylaşımlı spawn noktalarından en uygun olanını seçer.
	/// Test oyuncusunun pozisyonunu seçilen spawn noktasına atar.
	/// </summary>
    public void TestGetSpawnPoint()
    {
    	SpawnPoint spawnPoint = GetSharedSpawnPoint(PlayerToBeSpawned.PlayerTeamValue);
    	PlayerToBeSpawned.Transform.position = spawnPoint.PointTransform.position;
    }

}
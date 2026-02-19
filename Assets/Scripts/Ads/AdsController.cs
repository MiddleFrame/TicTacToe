using UnityEngine;

namespace Ads
{
    public class AdsController : MonoBehaviour
    {
        private const string ADUnitRewardCheep = "ca-app-pub-8340576279106634/2054300816";
       
        private void Start()
        {

            Debug.Log("Ads init successful");
        }

        
        

        //private IEnumerator TryToLoadRewardVideo(Action action = null)
        //{
        //    if (_rewardLoadingScreen == null) { _rewardLoadingScreen = Instantiate(_rewardLoadingScreenPrefab, GameObject.FindGameObjectWithTag("Boards").transform); }
        //    else
        //    {
        //        _rewardLoadingScreen.SetActive(true);
        //    }
        //    for (int i = 0; i < 5; i++)
        //    {
        //        if (_rewardedAd.IsLoaded())
        //        {
        //            _rewardedAd.Show();
        //            _actionEarn = action;
        //            _rewardLoadingScreen.SetActive(false);
        //            _load = null;
        //            yield break;
        //        }
        //        yield return new WaitForSeconds(0.5f);
        //    }
        //    _rewardLoadingScreen.SetActive(false);
        //    _load = null;
        //    if (_error == null) { _error = Instantiate(_errorPrefab, GameObject.FindGameObjectWithTag("Boards").transform); }
        //    else
        //    {
        //        _error.SetActive(true);
        //    }
        //    yield return new WaitForSeconds(2f);
        //    _error.SetActive(false);
        //}
    }
}
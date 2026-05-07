using Cards.Interfaces;
using DevMode.Interfaces;

namespace DevMode
{
    public class DevCollectionService : IDevCollectionService
    {
        private readonly ICollectionService _collectionService;

        public DevCollectionService(ICollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        public void UnlockAllCards()
        {
            _collectionService.UnlockAllCard();
        }

        public void LockAllCards()
        {
            _collectionService.LockAllCard();
        }
    }
}

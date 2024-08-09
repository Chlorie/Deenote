using Deenote.Localization;
using Deenote.UI.Windows.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class AboutWindow : MonoBehaviour
    {
        [SerializeField] Window _window;

        [SerializeField] AboutPageController _developersPage;
        [SerializeField] AboutPageController _updateHistoryPage;
        [SerializeField] AboutPageController _tutorialsPage;

        [SerializeField] ToggleGroup _sectionToggleGroup;
        [SerializeField] LocalizedText _contentText;

        [Header("Prefabs")]
        [SerializeField] AboutSectionController _sectionPrefab;
        [SerializeField] Transform _sectionParentTransform;
        private PooledObjectListView<AboutSectionController> _sections;

        private void Awake()
        {
            _sections = new PooledObjectListView<AboutSectionController>(UnityUtils.CreateObjectPool(() =>
            {
                var item = Instantiate(_sectionPrefab, _sectionParentTransform);
                item.OnCreated(_contentText, _sectionToggleGroup);
                return item;
            }));
        }

        public void LoadPage(AboutPageController page)
        {
            _sections.SetCount(page.Sections.Length);

            for (int i = 0; i < page.Sections.Length; i++) {
                var values = page.Sections[i];
                _sections[i].Initialize(
                    LocalizableText.Localized(values.SectionTextKey),
                    LocalizableText.Localized(values.ContentTextKey));
            }

            if (_sections.Count > 0) {
                _sections[0].Select();
            }
            else {
                _contentText.SetRawText("");
            }
        }
    }
}
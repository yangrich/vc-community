﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using VirtoCommerce.Foundation.AppConfig.Factories;
using VirtoCommerce.Foundation.AppConfig.Model;
using VirtoCommerce.Foundation.AppConfig.Repositories;
using VirtoCommerce.Foundation.Catalogs.Model;
using VirtoCommerce.Foundation.Frameworks;
using VirtoCommerce.Foundation.Frameworks.Extensions;
using Omu.ValueInjecter;
using VirtoCommerce.Foundation.Stores.Model;
using VirtoCommerce.Foundation.Stores.Repositories;
using VirtoCommerce.ManagementClient.Catalog.ViewModel.Catalog.Interfaces;
using VirtoCommerce.ManagementClient.Core.Infrastructure;
using catalogModel = VirtoCommerce.Foundation.Catalogs.Model;

namespace VirtoCommerce.ManagementClient.Catalog.ViewModel.Catalog.Implementations
{
	public class CategorySeoStepViewModel : CategoryViewModel, ICategorySeoStepViewModel
	{
		#region Dependencies

		private readonly IRepositoryFactory<IAppConfigRepository> _appConfigRepositoryFactory;
		private readonly IAppConfigEntityFactory _appConfigEntityFactory;
		private readonly IRepositoryFactory<IStoreRepository> _storeRepositoryFactory;
		#endregion

		public CategorySeoStepViewModel(IRepositoryFactory<IStoreRepository> storeRepositoryFactory, IRepositoryFactory<IAppConfigRepository> appConfigRepositoryFactory, IAppConfigEntityFactory appConfigEntityFactory, Category item, IEnumerable<string> languages, CatalogBase parentCatalog)
			: base(null, null, null, item, parentCatalog)
		{
			_appConfigRepositoryFactory = appConfigRepositoryFactory;
			_appConfigEntityFactory = appConfigEntityFactory;
			_storeRepositoryFactory = storeRepositoryFactory;
			InnerItemCatalogLanguages = languages.ToList();
			SeoLocalesFilterCommand = new DelegateCommand<string>(RaiseSeoLocalesFilter);
		}
				
		#region IWizardStep Members

		public override bool IsValid
		{
			get
			{
				return ValidateKeywords() && (!_seoModified || SeoKeywords.All(keyword => keyword.Validate() || (string.IsNullOrEmpty(keyword.Keyword) && string.IsNullOrEmpty(keyword.ImageAltDescription) && string.IsNullOrEmpty(keyword.Title) && string.IsNullOrEmpty(keyword.MetaDescription))));
			}
		}

		public override bool IsLast
		{
			get
			{
				return false;
			}
		}

		public override string Description
		{
			get
			{
				return "Enter category SEO information.";
			}
		}
		#endregion

		#region SEO tab
		
		public List<SeoUrlKeyword> SeoKeywords { get; private set; }

		private SeoUrlKeyword _currentSeoKeyword;
		public SeoUrlKeyword CurrentSeoKeyword
		{
			get { return _currentSeoKeyword; }
			set
			{
				_currentSeoKeyword = value;
				OnPropertyChanged();
			}
		}

		private bool _useCustomMetaDescription;
		public bool UseCustomMetaDescription
		{
			get { return _useCustomMetaDescription || !string.IsNullOrEmpty(CurrentSeoKeyword.MetaDescription); }
			set
			{
				_useCustomMetaDescription = value;
				if (!_useCustomMetaDescription && !string.IsNullOrEmpty(CurrentSeoKeyword.MetaDescription))
					CurrentSeoKeyword.MetaDescription = null;
				OnPropertyChanged();
			}
		}

		private bool _useCustomTitle;
		public bool UseCustomTitle
		{
			get { return _useCustomTitle || !string.IsNullOrEmpty(CurrentSeoKeyword.Title); }
			set
			{
				_useCustomTitle = value;
				if (!_useCustomTitle && !string.IsNullOrEmpty(CurrentSeoKeyword.Title))				
					CurrentSeoKeyword.Title = null;
				OnPropertyChanged();				
			}
		}

		private bool _useCustomImageText;
		public bool UseCustomImageText
		{
			get { return _useCustomImageText || !string.IsNullOrEmpty(CurrentSeoKeyword.ImageAltDescription); }
			set
			{
				_useCustomImageText = value;
				if (!_useCustomImageText && !string.IsNullOrEmpty(CurrentSeoKeyword.ImageAltDescription))
					CurrentSeoKeyword.ImageAltDescription = null;
				OnPropertyChanged();
			}
		}

		private void RaiseSeoLocalesFilter(string locale)
		{
			//detach property changed
			if (CurrentSeoKeyword != null)
				CurrentSeoKeyword.PropertyChanged -= CurrentSeoKeyword_PropertyChanged;

			CurrentSeoKeyword =
				SeoKeywords.FirstOrDefault(keyword => keyword.Language.Equals(locale, StringComparison.InvariantCultureIgnoreCase) && keyword.IsActive);

			if (CurrentSeoKeyword == null)
			{
				CurrentSeoKeyword = CreateSeoUrlKeyword(locale);
				SeoKeywords.Add(CurrentSeoKeyword);
			}

			//attach property changed
			CurrentSeoKeyword.PropertyChanged += CurrentSeoKeyword_PropertyChanged;

			FilterSeoLanguage = locale;
		}
		
		void CurrentSeoKeyword_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			_seoModified = true;
			OnViewModelPropertyChangedUI(null, null);
		}

		public void UpdateSeoKeywords()
		{
			//if any SEO keyword modified update or add it
			if (_seoModified)
			{
				using (var appConfigRepository = _appConfigRepositoryFactory.GetRepositoryInstance())
				{
					SeoKeywords.ForEach(keyword =>
					{
						if (!string.IsNullOrEmpty(keyword.Keyword))
						{
							var originalKeyword =
								appConfigRepository.SeoUrlKeywords.Where(
									seoKeyword =>
									seoKeyword.KeywordValue.Equals(keyword.KeywordValue) && seoKeyword.Language.Equals(keyword.Language) && seoKeyword.IsActive)
												   .FirstOrDefault();

							if (originalKeyword != null)
							{
								originalKeyword.InjectFrom(keyword);
								appConfigRepository.Update(originalKeyword);
							}
							else
							{
								var addKeyword = CreateSeoUrlKeyword(keyword.Language);
								addKeyword.InjectFrom(keyword);
								appConfigRepository.Add(addKeyword);
							}
						}
					});
					appConfigRepository.UnitOfWork.Commit();
				}
				_seoModified = false;
			}
		}

		private bool _seoModified;

		private string _filterSeoLanguage;
		public string FilterSeoLanguage 
		{ 
			get
			{
				return _filterSeoLanguage;
			}
			private set
			{
				_filterSeoLanguage = value;
				OnPropertyChanged();

				OnPropertyChanged("UseCustomTitle");
				OnPropertyChanged("UseCustomMetaDescription");
				OnPropertyChanged("UseCustomImageText");
			}
		}
		public DelegateCommand<string> SeoLocalesFilterCommand { get; private set; }

		#endregion

		protected override void InitializePropertiesForViewing()
		{
			using (var _appConfigRepository = _appConfigRepositoryFactory.GetRepositoryInstance())
			{
				SeoKeywords =
					_appConfigRepository.SeoUrlKeywords.Where(
						keyword =>
						keyword.KeywordValue.Equals(InnerItem.Code) && keyword.KeywordType.Equals((int)SeoUrlKeywordTypes.Category))
										.ToList();
			}

			var innerItemCatalog = _parentCatalog as catalogModel.Catalog;
			if (innerItemCatalog == null)
			{
				using (var storeRepository = _storeRepositoryFactory.GetRepositoryInstance())
				{
					var languages =
						storeRepository.Stores.Where(store => store.Catalog == _parentCatalog.CatalogId)
						               .Expand(store => store.Languages).ToList();

					var customComparer = new PropertyComparer<StoreLanguage>("LanguageCode");
					var lang = languages.SelectMany(x => x.Languages).Distinct(customComparer);

					InnerItemCatalogLanguages = new List<string>();
					if (lang.Any())
					{
						foreach (var l in lang)
						{
							InnerItemCatalogLanguages.Add(l.LanguageCode);
						}
					}
				}
			}
			
			InnerItemCatalogLanguages.ForEach(locale =>
			{
				if (!SeoKeywords.Any(keyword => keyword.Language.Equals(locale)))
				{
					SeoKeywords.Add(CreateSeoUrlKeyword(locale));
				}
			});

			// filter values by locale
			SeoLocalesFilterCommand.Execute(_parentCatalog.DefaultLanguage);
		}

		#region Auxilliary methods

		private bool ValidateKeywords()
		{
			var retVal = true;
			var keywords = SeoKeywords.Where(key => !string.IsNullOrEmpty(key.Keyword)).ToList();
			if (keywords.Any())
			{
				using (var appConfigRepository = _appConfigRepositoryFactory.GetRepositoryInstance())
				{
					keywords.ForEach(keyword =>
					{
						if (retVal)
						{
							var count = appConfigRepository.SeoUrlKeywords
														   .Where(x =>
																  x.SeoUrlKeywordId != keyword.SeoUrlKeywordId &&
																  x.Keyword == keyword.Keyword && x.KeywordType == keyword.KeywordType && x.Language == keyword.Language && x.IsActive == keyword.IsActive)
														   .Count();

							if (count > 0)
							{
								keyword.SetError("Keyword", "Category with the same Keyword and Language already exists", true);
								CurrentSeoKeyword = keyword;
								RaiseSeoLocalesFilter(keyword.Language);
								retVal = false;
							}
						}
					});
				}
			}

			return retVal;
		}

		private SeoUrlKeyword CreateSeoUrlKeyword(string locale)
		{
			var newSeoKeyword = _appConfigEntityFactory.CreateEntity<SeoUrlKeyword>();
			newSeoKeyword.Language = locale;
			newSeoKeyword.IsActive = true;
			newSeoKeyword.KeywordType = (int)SeoUrlKeywordTypes.Category;
			newSeoKeyword.KeywordValue = InnerItem.Code;
			newSeoKeyword.Created = DateTime.UtcNow;

			return newSeoKeyword;
		}

		#endregion
	}
}

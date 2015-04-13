﻿using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Marketing.Services;
using VirtoCommerce.Foundation.Frameworks.Caching;
using VirtoCommerce.MarketingModule.Data.Repositories;
using foundationModel = VirtoCommerce.Foundation.Marketing.Model;
using coreModel = VirtoCommerce.Domain.Marketing.Model;
using VirtoCommerce.Foundation.Data.Infrastructure;
using VirtoCommerce.Foundation.Frameworks.Extensions;
using Omu.ValueInjecter;
using VirtoCommerce.CustomerModule.Data.Converters;
using ExpressionSerialization;

namespace VirtoCommerce.MarketingModule.Data.Services
{
	public class MarketingSearchServiceImpl : IMarketingSearchService
	{
		private readonly Func<IFoundationPromotionRepository> _promotionRepositoryFactory;
		private readonly Func<IFoundationDynamicContentRepository> _contentRepositoryFactory;
		private readonly IPromotionExtensionManager _customPromotionManager;

		public MarketingSearchServiceImpl(Func<IFoundationPromotionRepository> repositoryFactory, Func<IFoundationDynamicContentRepository> contentRepositoryFactory, IPromotionExtensionManager customPromotionManager)
		{
			_promotionRepositoryFactory = repositoryFactory;
			_contentRepositoryFactory = contentRepositoryFactory;
			_customPromotionManager = customPromotionManager;
		}


		#region IMarketingSearchService Members

		public coreModel.MarketingSearchResult SearchResources(coreModel.MarketingSearchCriteria criteria)
		{
			var retVal = new coreModel.MarketingSearchResult();
			var count = criteria.Count;
			if ((criteria.ResponseGroup & coreModel.SearchResponseGroup.WithPromotions) == coreModel.SearchResponseGroup.WithPromotions)
			{
				SearchPromotions(criteria, retVal);
				criteria.Count -= retVal.Promotions.Count();
			}
			if ((criteria.ResponseGroup & coreModel.SearchResponseGroup.WithContentItems) == coreModel.SearchResponseGroup.WithContentItems)
			{
				SearchContentItems(criteria, retVal);
				criteria.Count -= retVal.ContentItems.Count();
			}
			if ((criteria.ResponseGroup & coreModel.SearchResponseGroup.WithContentPlaces) == coreModel.SearchResponseGroup.WithContentPlaces)
			{
				SearchContentPlaces(criteria, retVal);
				criteria.Count -= retVal.ContentPlaces.Count();
			}
			if ((criteria.ResponseGroup & coreModel.SearchResponseGroup.WithContentPublications) == coreModel.SearchResponseGroup.WithContentPublications)
			{
				SearchContentPublications(criteria, retVal);
				criteria.Count -= retVal.ContentPublications.Count();
			}
			return retVal;
		}

		#endregion

		private void SearchContentItems(coreModel.MarketingSearchCriteria criteria, coreModel.MarketingSearchResult result)
		{
			var totalCount = 0;
			using (var repository = _contentRepositoryFactory())
			{
				var query = repository.Items;
				totalCount = query.Count();

				result.ContentItems = query.OrderBy(x => x.DynamicContentItemId)
											  .Skip(criteria.Start)
											  .Take(criteria.Count)
											  .ToArray()
											  .Select(x => x.ToCoreModel())
											  .ToList();
			}

		}

		private void SearchContentPlaces(coreModel.MarketingSearchCriteria criteria, coreModel.MarketingSearchResult result)
		{
			var totalCount = 0;
			using (var repository = _contentRepositoryFactory())
			{
				var query = repository.Places;
				totalCount = query.Count();

				result.ContentPlaces = query.OrderBy(x => x.DynamicContentPlaceId)
											  .Skip(criteria.Start)
											  .Take(criteria.Count)
											  .ToArray()
											  .Select(x => x.ToCoreModel())
											  .ToList();
			}

		}

		private void SearchContentPublications(coreModel.MarketingSearchCriteria criteria, coreModel.MarketingSearchResult result)
		{
			var totalCount = 0;
			using (var repository = _contentRepositoryFactory())
			{
				var query = repository.PublishingGroups;
				totalCount = query.Count();

				result.ContentPublications = query.OrderBy(x => x.DynamicContentPublishingGroupId)
											  .Skip(criteria.Start)
											  .Take(criteria.Count)
											  .ToArray()
											  .Select(x => x.ToCoreModel())
											  .ToList();
			}

		}
		private void SearchPromotions(coreModel.MarketingSearchCriteria criteria, coreModel.MarketingSearchResult result)
		{
			var promotions = new List<coreModel.Promotion>();
			var totalCount = 0;
			using (var repository = _promotionRepositoryFactory())
			{
				promotions = repository.Promotions.OrderBy(x => x.PromotionId)
											  .Skip(criteria.Start)
											  .Take(criteria.Count)
											  .ToArray()
											  .Select(x => x.ToCoreModel())
											  .ToList();
				totalCount = repository.Promotions.Count();
			}


			promotions.AddRange(_customPromotionManager.Promotions.Skip(criteria.Start).Take(criteria.Count));
			totalCount += _customPromotionManager.Promotions.Count();


			var retVal = new coreModel.MarketingSearchResult
			{
				Promotions = promotions.OrderBy(x => x.Id).Take(criteria.Count).ToList(),
				TotalCount = totalCount
			};
			result.Promotions = promotions;
			result.TotalCount += totalCount;
		}
	}
}
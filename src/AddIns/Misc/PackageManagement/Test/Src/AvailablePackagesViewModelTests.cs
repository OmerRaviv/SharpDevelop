﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using ICSharpCode.PackageManagement;
using ICSharpCode.PackageManagement.Design;
using NuGet;
using NUnit.Framework;
using PackageManagement.Tests.Helpers;

namespace PackageManagement.Tests
{
	[TestFixture]
	public class AvailablePackagesViewModelTests
	{
		AvailablePackagesViewModel viewModel;
		FakeRegisteredPackageRepositories registeredPackageRepositories;
		ExceptionThrowingRegisteredPackageRepositories exceptionThrowingRegisteredPackageRepositories;
		FakeTaskFactory taskFactory;
		
		void CreateViewModel()
		{
			CreateRegisteredPackageRepositories();
			CreateViewModel(registeredPackageRepositories);
		}
		
		void CreateRegisteredPackageRepositories()
		{
			registeredPackageRepositories = new FakeRegisteredPackageRepositories();
		}
		
		void CreateViewModel(FakeRegisteredPackageRepositories registeredPackageRepositories)
		{
			taskFactory = new FakeTaskFactory();
			var packageViewModelFactory = new FakePackageViewModelFactory();
			viewModel = new AvailablePackagesViewModel(registeredPackageRepositories, packageViewModelFactory, taskFactory);
		}
		
		void CreateExceptionThrowingRegisteredPackageRepositories()
		{
			exceptionThrowingRegisteredPackageRepositories = new ExceptionThrowingRegisteredPackageRepositories();
		}
		
		void CompleteReadPackagesTask()
		{
			taskFactory.ExecuteAllFakeTasks();
		}
		
		void ClearReadPackagesTasks()
		{
			taskFactory.ClearAllFakeTasks();
		}

		void AddOnePackageSourceToRegisteredSources()
		{
			registeredPackageRepositories.ClearPackageSources();
			registeredPackageRepositories.AddOnePackageSource();
			registeredPackageRepositories.HasMultiplePackageSources = false;
		}
		
		void AddTwoPackageSourcesToRegisteredSources()
		{
			var expectedPackageSources = new PackageSource[] {
				new PackageSource("http://first.com", "First"),
				new PackageSource("http://second.com", "Second")
			};
			AddPackageSourcesToRegisteredSources(expectedPackageSources);
			registeredPackageRepositories.HasMultiplePackageSources = true;
		}
				
		void AddPackageSourcesToRegisteredSources(PackageSource[] sources)
		{
			registeredPackageRepositories.ClearPackageSources();
			registeredPackageRepositories.AddPackageSources(sources);
		}
		
		void CreateNewActiveRepositoryWithDifferentPackages()
		{
			var package = new FakePackage("NewRepositoryPackageId");
			var newRepository = new FakePackageRepository();
			newRepository.FakePackages.Add(package);
			registeredPackageRepositories.FakeActiveRepository = newRepository;
		}
		
		void SetUpTwoPackageSourcesAndViewModelHasReadPackages()
		{
			CreateRegisteredPackageRepositories();
			AddTwoPackageSourcesToRegisteredSources();
			CreateViewModel(registeredPackageRepositories);
			registeredPackageRepositories.ActivePackageSource = registeredPackageRepositories.PackageSources[0];
			viewModel.ReadPackages();
			CompleteReadPackagesTask();
			CreateNewActiveRepositoryWithDifferentPackages();
		}
		
		void ChangeSelectedPackageSourceToSecondSource()
		{
			var secondPackageSource = registeredPackageRepositories.PackageSources[1];
			viewModel.SelectedPackageSource = secondPackageSource;
		}
		
		void ChangeSelectedPackageSourceToFirstSource()
		{
			var firstPackageSource = registeredPackageRepositories.PackageSources[0];
			viewModel.SelectedPackageSource = firstPackageSource;
		}
		
		[Test]
		public void ReadPackages_RepositoryHasThreePackagesWithSameIdButDifferentVersions_HasLatestPackageVersionOnly()
		{
			CreateViewModel();
			
			var package1 = new FakePackage() {
             	Id = "Test",
             	Version = new Version(0, 1, 0, 0)
            };
			var package2 = new FakePackage() {
				Id = "Test",
				Version = new Version(0, 2, 0, 0)
			};
			var package3 = new FakePackage() {
				Id = "Test",
				Version = new Version(0, 3, 0, 0)
			};
			var packages = new FakePackage[] {
				package1, package2, package3
			};
			
			registeredPackageRepositories.FakeActiveRepository.FakePackages.AddRange(packages);
			
			viewModel.ReadPackages();
			CompleteReadPackagesTask();
			
			var expectedPackages = new FakePackage[] {
				package3
			};
			
			PackageCollectionAssert.AreEqual(expectedPackages, viewModel.PackageViewModels);
		}
		
		[Test]
		public void IsSearchable_ByDefault_ReturnsTrue()
		{
			CreateViewModel();
			Assert.IsTrue(viewModel.IsSearchable);
		}
	
		[Test]
		public void Search_RepositoryHasThreePackagesWithSameIdButSearchTermsMatchNoPackageIds_ReturnsNoPackages()
		{
			CreateViewModel();
			
			var package1 = new FakePackage() {
             	Id = "Test",
             	Description = "",
             	Version = new Version(0, 1, 0, 0)
            };
			var package2 = new FakePackage() {
				Id = "Test",
             	Description = "",
				Version = new Version(0, 2, 0, 0)
			};
			var package3 = new FakePackage() {
				Id = "Test",
             	Description = "",
				Version = new Version(0, 3, 0, 0)
			};
			var packages = new FakePackage[] {
				package1, package2, package3
			};
			
			registeredPackageRepositories.FakeActiveRepository.FakePackages.AddRange(packages);
			
			viewModel.ReadPackages();
			CompleteReadPackagesTask();
			
			ClearReadPackagesTasks();
			viewModel.SearchTerms = "NotAMatch";
			viewModel.Search();
			CompleteReadPackagesTask();
			
			Assert.AreEqual(0, viewModel.PackageViewModels.Count);
		}
		
		[Test]
		public void ShowNextPage_TwoObjectsWatchingForPagesCollectionChangedEventAndUserMovesToPageTwoAndFilteredPackagesReturnsLessThanExpectedPackagesDueToMatchingVersions_InvalidOperationExceptionNotThrownWhen()
		{
			CreateViewModel();
			viewModel.PageSize = 2;
			
			var package1 = new FakePackage() {
             	Id = "First",
             	Description = "",
             	Version = new Version(0, 1, 0, 0)
            };
			var package2 = new FakePackage() {
				Id = "Secon",
             	Description = "",
				Version = new Version(0, 2, 0, 0)
			};
			var package3 = new FakePackage() {
				Id = "Test",
             	Description = "",
				Version = new Version(0, 3, 0, 0)
			};
			var package4 = new FakePackage() {
				Id = "Test",
             	Description = "",
				Version = new Version(0, 4, 0, 0)
			};
			var packages = new FakePackage[] {
				package1, package2, package3, package4
			};
			
			registeredPackageRepositories.FakeActiveRepository.FakePackages.AddRange(packages);
			
			viewModel.ReadPackages();
			CompleteReadPackagesTask();
			
			ClearReadPackagesTasks();
			bool collectionChangedEventFired = false;
			viewModel.Pages.CollectionChanged += (sender, e) => collectionChangedEventFired = true;
			viewModel.ShowNextPage();
			CompleteReadPackagesTask();
			
			var expectedPackages = new FakePackage[] {
				package4
			};
			
			PackageCollectionAssert.AreEqual(expectedPackages, viewModel.PackageViewModels);
			Assert.IsTrue(collectionChangedEventFired);
		}
		
		[Test]
		public void ShowSources_TwoPackageSources_ReturnsTrue()
		{
			CreateRegisteredPackageRepositories();
			AddTwoPackageSourcesToRegisteredSources();
			CreateViewModel(registeredPackageRepositories);
			
			Assert.IsTrue(viewModel.ShowPackageSources);
		}
		
		[Test]
		public void ShowPackageSources_OnePackageSources_ReturnsFalse()
		{
			CreateRegisteredPackageRepositories();
			AddOnePackageSourceToRegisteredSources();
			CreateViewModel(registeredPackageRepositories);
			
			Assert.IsFalse(viewModel.ShowPackageSources);
		}
		
		[Test]
		public void PackageSources_TwoPackageSourcesInOptions_ReturnsTwoPackageSourcesPlusAggregatePackageSource()
		{
			CreateRegisteredPackageRepositories();
			AddTwoPackageSourcesToRegisteredSources();
			CreateViewModel(registeredPackageRepositories);
			
			var expectedPackageSources = new List<PackageSource>(registeredPackageRepositories.PackageSources);
			expectedPackageSources.Add(RegisteredPackageSourceSettings.AggregatePackageSource);
			
			PackageSourceCollectionAssert.AreEqual(expectedPackageSources, viewModel.PackageSources);
		}
		
		[Test]
		public void PackageSources_OnePackageSourceInOptions_ReturnsOnePackageSource()
		{
			CreateRegisteredPackageRepositories();
			AddOnePackageSourceToRegisteredSources();
			CreateViewModel(registeredPackageRepositories);
			
			var expectedPackageSources = new List<PackageSource>(registeredPackageRepositories.PackageSources);
			
			PackageSourceCollectionAssert.AreEqual(expectedPackageSources, viewModel.PackageSources);
		}
		
		[Test]
		public void SelectedPackageSource_TwoPackageSourcesInOptionsAndActivePackageSourceIsFirstSource_IsFirstPackageSource()
		{
			CreateRegisteredPackageRepositories();
			AddTwoPackageSourcesToRegisteredSources();
			CreateViewModel(registeredPackageRepositories);
			
			var expectedPackageSource = registeredPackageRepositories.PackageSources[0];
			registeredPackageRepositories.ActivePackageSource = expectedPackageSource;
			
			Assert.AreEqual(expectedPackageSource, viewModel.SelectedPackageSource);
		}
		
		[Test]
		public void SelectedPackageSource_TwoPackageSourcesInOptionsAndActivePackageSourceIsSecondSource_IsSecondPackageSource()
		{
			CreateRegisteredPackageRepositories();
			AddTwoPackageSourcesToRegisteredSources();
			CreateViewModel(registeredPackageRepositories);
			
			var expectedPackageSource = registeredPackageRepositories.PackageSources[1];
			registeredPackageRepositories.ActivePackageSource = expectedPackageSource;
			
			Assert.AreEqual(expectedPackageSource, viewModel.SelectedPackageSource);
		}
		
		[Test]
		public void SelectedPackageSource_Changed_ActivePackageSourceChanged()
		{
			CreateRegisteredPackageRepositories();
			AddTwoPackageSourcesToRegisteredSources();
			CreateViewModel(registeredPackageRepositories);
			
			registeredPackageRepositories.ActivePackageSource = registeredPackageRepositories.PackageSources[0];
			var expectedPackageSource = registeredPackageRepositories.PackageSources[1];
			viewModel.SelectedPackageSource = expectedPackageSource;
			
			Assert.AreEqual(expectedPackageSource, registeredPackageRepositories.ActivePackageSource);
		}
		
		[Test]
		public void SelectedPackageSource_PackageSourceChangedAfterReadingPackages_PackagesReadFromNewPackageSourceAndDisplayed()
		{
			SetUpTwoPackageSourcesAndViewModelHasReadPackages();
			ClearReadPackagesTasks();
			ChangeSelectedPackageSourceToSecondSource();
			CompleteReadPackagesTask();
			
			var expectedPackages = registeredPackageRepositories.FakeActiveRepository.FakePackages;
			
			PackageCollectionAssert.AreEqual(expectedPackages, viewModel.PackageViewModels);
		}
		
		[Test]
		public void SelectedPackageSource_PackageSourceChangedAfterReadingPackages_PropertyChangedEventFiredAfterPackagesAreRead()
		{
			SetUpTwoPackageSourcesAndViewModelHasReadPackages();
			
			int packageCountWhenPropertyChangedEventFired = -1;
			viewModel.PropertyChanged += (sender, e) => packageCountWhenPropertyChangedEventFired = viewModel.PackageViewModels.Count;
			ClearReadPackagesTasks();
			ChangeSelectedPackageSourceToSecondSource();
			CompleteReadPackagesTask();
			
			Assert.AreEqual(1, packageCountWhenPropertyChangedEventFired);
		}
		
		[Test]
		public void SelectedPackageSource_PackageSourceChangedButToSameSelectedPackageSource_PackagesAreNotRead()
		{
			SetUpTwoPackageSourcesAndViewModelHasReadPackages();
			ChangeSelectedPackageSourceToFirstSource();
			
			Assert.AreEqual(0, viewModel.PackageViewModels.Count);
		}
		
		[Test]
		public void SelectedPackageSource_PackageSourceChangedButToSameSelectedPackageSource_PropertyChangedEventNotFired()
		{
			SetUpTwoPackageSourcesAndViewModelHasReadPackages();
			
			bool fired = false;
			viewModel.PropertyChanged += (sender, e) => fired = true;
			ChangeSelectedPackageSourceToFirstSource();
			
			Assert.IsFalse(fired);
		}
		
		[Test]
		public void GetAllPackages_OnePackageInRepository_RepositoryNotCreatedByBackgroundThread()
		{
			CreateRegisteredPackageRepositories();
			AddOnePackageSourceToRegisteredSources();
			registeredPackageRepositories.FakeActiveRepository.FakePackages.Add(new FakePackage());
			CreateViewModel(registeredPackageRepositories);
			viewModel.ReadPackages();
			
			registeredPackageRepositories.FakeActiveRepository = null;
			CompleteReadPackagesTask();
			
			Assert.AreEqual(1, viewModel.PackageViewModels.Count);
		}
		
		[Test]
		public void ReadPackages_ExceptionThrownWhenAccessingActiveRepository_ErrorMessageFromExceptionNotOverriddenByReadPackagesCall()
		{
			CreateExceptionThrowingRegisteredPackageRepositories();
			exceptionThrowingRegisteredPackageRepositories.ExeptionToThrowWhenActiveRepositoryAccessed = 
				new Exception("Test");
			CreateViewModel(exceptionThrowingRegisteredPackageRepositories);
			viewModel.ReadPackages();
			
			ApplicationException ex = Assert.Throws<ApplicationException>(() => CompleteReadPackagesTask());
			Assert.AreEqual("Test", ex.Message);
		}
	}
}

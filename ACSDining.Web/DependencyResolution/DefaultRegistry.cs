// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Web.Http;
using ACSDining.Core.DataContext;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Core.UnitOfWork;
using ACSDining.Infrastructure.DAL;
using ACSDining.Infrastructure.Identity;
using ACSDining.Service;
using StructureMap.Web.Pipeline;

namespace ACSDining.Web.DependencyResolution {
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;
	
    public class DefaultRegistry : Registry {
        #region Constructors and Destructors

        public DefaultRegistry() {
            Scan(
                scan => {
                    scan.TheCallingAssembly();
                    scan.AssemblyContainingType<UnitOfWork>();
                    scan.WithDefaultConventions();
					scan.With(new ControllerConvention());
                });

            //For<IExample>().Use<Example>();
            For<IDataContextAsync>().Use<DataContext>();
            For<IUnitOfWork>().Use<UnitOfWork>();
            For<Microsoft.AspNet.Identity.IUserStore<User>>()
            .Use<Microsoft.AspNet.Identity.EntityFramework.UserStore<User>>();

            For<IMenuForWeekService>().Use<MenuForWeekService>();
            For<IRepositoryAsync<MenuForWeek>>().Use<Repository<MenuForWeek>>();

            For<IOrderMenuService>().Use<OrderMenuService>();
            For<IRepositoryAsync<OrderMenu>>().Use<Repository<OrderMenu>>();

            For<IGetExcelService>().Use<GetExcelService>();

            For<IUserAccountService>().Use<UserAccountService>();
            For<IRepositoryAsync<User>>().Use<Repository<User>>();

            For<IWorkDaysService>().Use<WorkDaysService>();
            For<IRepositoryAsync<WorkingWeek>>().Use<Repository<WorkingWeek>>();

            For<IDishService>().Use<DishService>();
            For<IRepositoryAsync<Dish>>().Use<Repository<Dish>>();
        }

        #endregion
    }
}
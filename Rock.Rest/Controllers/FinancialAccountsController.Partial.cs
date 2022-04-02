// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Linq;
using System.Web;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FinancialAccountsController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <param name="keyword">The keyword.</param>
        /// <returns>IQueryable&lt;TreeViewItem&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildrenByKeyword/{id}/{activeOnly}/{keyword}" )]
        public IQueryable<AccountTreeViewItem> GetChildren( int id, bool activeOnly, string keyword )
        {
            return GetChildrenData( id, activeOnly, true, keyword );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildren/{id}/{activeOnly}" )]
        public IQueryable<AccountTreeViewItem> GetChildren( int id, bool activeOnly )
        {
            return GetChildrenData( id, activeOnly, true );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="activeOnly">if set to <c>true</c> [active only].</param>
        /// <param name="displayPublicName">if set to <c>true</c> [public name].</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetChildren/{id}/{activeOnly}/{displayPublicName}" )]
        public IQueryable<AccountTreeViewItem> GetChildren( int id, bool activeOnly, bool displayPublicName )
        {
            return GetChildrenData( id, activeOnly, displayPublicName );
        }

        /// <summary>
        /// Gets the inactive.
        /// </summary>
        /// <param name="displayPublicName">if set to <c>true</c> [display public name].</param>
        /// <returns>IQueryable&lt;TreeViewItem&gt;.</returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/FinancialAccounts/GetInactive/{displayPublicName}" )]
        public IQueryable<AccountTreeViewItem> GetInactive( bool displayPublicName )
        {
            IQueryable<FinancialAccount> qry;


            qry = Get().Where( f =>
                f.ParentAccountId.HasValue == false );


            qry = qry
                .Where( f => f.IsActive == false );


            var accountList = qry
                .OrderBy( f => f.Order )
                .ThenBy( f => f.Name )
                .ToList();

            var accountTreeViewItems = accountList.Select( a => new AccountTreeViewItem
            {
                Id = a.Id.ToString(),
                Name = HttpUtility.HtmlEncode( displayPublicName ? a.PublicName : a.Name ),
                GlCode = a.GlCode,
                IsActive = a.IsActive
            } ).ToList();

            var resultIds = accountList.Select( f => f.Id ).ToList();

            var childrenList = Get()
                .Where( f =>
                    f.ParentAccountId.HasValue &&
                    resultIds.Contains( f.ParentAccountId.Value ) )
                .Select( f => f.ParentAccountId.Value )
                .ToList();

            foreach ( var accountTreeViewItem in accountTreeViewItems )
            {
                int accountId = int.Parse( accountTreeViewItem.Id );
                int childrenCount = ( childrenList?.Count( v => v == accountId ) ).GetValueOrDefault( 0 );

                accountTreeViewItem.HasChildren = childrenCount > 0;
                var lastChildId = ( childrenList?.LastOrDefault() ).GetValueOrDefault( 0 );

                if ( accountTreeViewItem.HasChildren )
                {
                    accountTreeViewItem.CountInfo = childrenCount;
                    accountTreeViewItem.ParentId = accountId.ToString();

                }


                accountTreeViewItem.IconCssClass = "fa fa-file-o";
            }

            return accountTreeViewItems.AsQueryable();
        }

        #region Methods
        private IQueryable<AccountTreeViewItem> GetChildrenData( int id, bool activeOnly, bool displayPublicName, string keyword = "" )
        {
            IQueryable<FinancialAccount> qry;

            if ( keyword.IsNotNullOrWhiteSpace() )
            {
                var upperWord = keyword.ToUpper();
                qry = Get().Where( f =>
                    ( f.Name != null && f.Name.ToUpper().Contains( upperWord ) )
                    || ( f.PublicName != null && f.PublicName.ToUpper().Contains( upperWord ) )
                    || ( f.GlCode != null && f.GlCode.ToUpper().Contains( upperWord ) )
                    );
            }
            else if ( id == 0 )
            {
                qry = Get().Where( f =>
                    f.ParentAccountId.HasValue == false );
            }
            else
            {
                qry = Get().Where( f =>
                    f.ParentAccountId.HasValue &&
                    f.ParentAccountId.Value == id );
            }

            if ( activeOnly )
            {
                qry = qry
                    .Where( f => f.IsActive == activeOnly );
            }
                       
            var accountList = qry
                .OrderBy( f => f.Order )
                .ThenBy( f => f.Name )
                .ToList();

            var accountTreeViewItems = accountList.Select( a => new AccountTreeViewItem
            {
                Id = a.Id.ToString(),
                Name = HttpUtility.HtmlEncode( displayPublicName ? a.PublicName : a.Name ),
                GlCode = a.GlCode,
                IsActive = a.IsActive,
                IsKeyWordResult = keyword.IsNotNullOrWhiteSpace()
            } ).ToList();

            var resultIds = accountList.Select( f => f.Id ).ToList();

            var childrenList = Get()
                .Where( f =>
                    f.ParentAccountId.HasValue &&
                    resultIds.Contains( f.ParentAccountId.Value ) )
                .Select( f => f.ParentAccountId.Value )
                .ToList();

            foreach ( var accountTreeViewItem in accountTreeViewItems )
            {
                int accountId = int.Parse( accountTreeViewItem.Id );
                int childrenCount = ( childrenList?.Count( v => v == accountId ) ).GetValueOrDefault( 0 );

                accountTreeViewItem.HasChildren = childrenCount > 0;
                var lastChildId = ( childrenList?.LastOrDefault() ).GetValueOrDefault( 0 );

                if ( accountTreeViewItem.HasChildren )
                {
                    accountTreeViewItem.CountInfo = childrenCount;
                    accountTreeViewItem.ParentId = id.ToString();

                    // If this was an incoming keyword search we need the children to display in the search UI
                    if ( keyword.IsNotNullOrWhiteSpace() )
                    {
                        accountTreeViewItem.Children = this.GetChildrenData( accountId, activeOnly, displayPublicName ).ToList();
                    }
                }


                accountTreeViewItem.IconCssClass = "fa fa-file-o";
            }

            return accountTreeViewItems.AsQueryable();
        }
        #endregion Methods
    }
}

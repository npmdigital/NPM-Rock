using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "ABWETests" )]
    [Category( "Finance" )]
    [Description( "ABWE Tests" )]

    [AccountField( "My Account", "My Account Field", Key = "MyAccountField", EnhancedForLongLists =true )]
    
    public partial class ABWETests : RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            dynamic entityTypes = new EntityTypeService( new RockContext() ).GetEntities()
              .ToList();
                        
            //epEntityType.EntityTypes = entityTypes;
        }
    }
}
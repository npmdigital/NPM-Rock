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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.ViewModel.NonEntities;

namespace Rock.ViewModel.Blocks.Core.CampusDetail
{
    public class CampusViewModel : ViewModelBase
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool? IsActive { get; set; }

        public string ShortCode { get; set; }

        public string Url { get; set; }

        public string PhoneNumber { get; set; }

        public List<ListItemViewModel> ServiceTimes { get; set; }

        public string TimeZoneId { get; set; }

        public List<CampusScheduleViewModel> CampusSchedules { get; set; }

        public ListItemViewModel Location { get; set; }

        public ListItemViewModel LeaderPersonAlias { get; set; }

        public ListItemViewModel CampusStatusValue { get; set; }

        public ListItemViewModel CampusTypeValue { get; set; }
    }

    public class CampusScheduleViewModel
    {
        public Guid Guid { get; set; }

        public ListItemViewModel Schedule { get; set; }

        public ListItemViewModel ScheduleTypeValue { get; set; }
    }
}

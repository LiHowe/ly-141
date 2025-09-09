using System.ComponentModel;

namespace Core.Models
{
    /// <summary>
    /// 图标类型枚举
    /// </summary>
    public enum IconType
    {
        /// <summary>
        /// 文本图标（如Emoji、字符）
        /// </summary>
        Text,
        /// <summary>
        /// 图片图标（PNG、JPG等）
        /// </summary>
        Image
    }

    /// <summary>
    /// 菜单配置项
    /// </summary>
    public class MenuConfiguration : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private string _page = string.Empty;
        private string _role = "User";
        private bool _enabled = true;
        private string _icon = string.Empty;
        private IconType _iconType = IconType.Text;
        private int _sortOrder = 0;
        private string _parentId = string.Empty;
        private string _moduleId = string.Empty;

        /// <summary>
        /// 菜单唯一标识符
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        /// <summary>
        /// 菜单显示名称
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// 对应的页面类型名称或路径
        /// </summary>
        public string Page
        {
            get => _page;
            set
            {
                if (_page != value)
                {
                    _page = value;
                    OnPropertyChanged(nameof(Page));
                }
            }
        }

        /// <summary>
        /// 所需角色权限（User, Admin, SuperAdmin等）
        /// </summary>
        public string Role
        {
            get => _role;
            set
            {
                if (_role != value)
                {
                    _role = value;
                    OnPropertyChanged(nameof(Role));
                }
            }
        }

        /// <summary>
        /// 菜单是否启用
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        /// <summary>
        /// 菜单图标路径或字符
        /// </summary>
        public string Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        /// <summary>
        /// 图标类型（文本或图片）
        /// </summary>
        public IconType IconType
        {
            get => _iconType;
            set
            {
                if (_iconType != value)
                {
                    _iconType = value;
                    OnPropertyChanged(nameof(IconType));
                }
            }
        }

        /// <summary>
        /// 排序顺序
        /// </summary>
        public int SortOrder
        {
            get => _sortOrder;
            set
            {
                if (_sortOrder != value)
                {
                    _sortOrder = value;
                    OnPropertyChanged(nameof(SortOrder));
                }
            }
        }

        /// <summary>
        /// 父菜单ID（用于子菜单）
        /// </summary>
        public string ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    OnPropertyChanged(nameof(ParentId));
                }
            }
        }

        /// <summary>
        /// 所属模块ID
        /// </summary>
        public string ModuleId
        {
            get => _moduleId;
            set
            {
                if (_moduleId != value)
                {
                    _moduleId = value;
                    OnPropertyChanged(nameof(ModuleId));
                }
            }
        }

        /// <summary>
        /// 是否为根菜单
        /// </summary>
        public bool IsRootMenu => string.IsNullOrEmpty(ParentId);

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 菜单配置集合
    /// </summary>
    public class MenuConfigurationCollection
    {
        /// <summary>
        /// 菜单项列表
        /// </summary>
        public List<MenuConfiguration> Menus { get; set; } = new();

        /// <summary>
        /// 获取根菜单项
        /// </summary>
        public List<MenuConfiguration> GetRootMenus()
        {
            return Menus.Where(m => m.IsRootMenu).OrderBy(m => m.SortOrder).ToList();
        }

        /// <summary>
        /// 获取指定父菜单的子菜单
        /// </summary>
        public List<MenuConfiguration> GetChildMenus(string parentId)
        {
            return Menus.Where(m => m.ParentId == parentId).OrderBy(m => m.SortOrder).ToList();
        }

        /// <summary>
        /// 根据ID获取菜单项
        /// </summary>
        public MenuConfiguration? GetMenuById(string id)
        {
            return Menus.FirstOrDefault(m => m.Id == id);
        }

        /// <summary>
        /// 根据模块ID获取菜单项
        /// </summary>
        public List<MenuConfiguration> GetMenusByModuleId(string moduleId)
        {
            return Menus.Where(m => m.ModuleId == moduleId).OrderBy(m => m.SortOrder).ToList();
        }
    }
}

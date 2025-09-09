using System.Collections.ObjectModel;
using Connection.S7;
using UI.ViewModels;

namespace UI.Converters;

public static class S7NodeConverter
{
    public static S7NodeViewModel ToViewModel(S7PlcNode node)
    {
        if (node == null) return null;

        return new S7NodeViewModel
        {
            Type = node.Type,
            Db = node.Db,
            Offset = node.Offset,
            BitLength = node.BitLength,
            Description = node.Description,
            Title = node.Title,
            NeedFeedback = node.NeedFeedback,
            FeedbackNodeKey = node.FeedbackNodeKey,
            Enabled = node.Enabled
        };
    }

    public static S7PlcNode ToNode(S7NodeViewModel viewModel)
    {
        if (viewModel == null) return null;

        return new S7PlcNode
        {
            Type = viewModel.Type,
            Db = viewModel.Db,
            Offset = viewModel.Offset,
            BitLength = viewModel.BitLength,
            Description = viewModel.Description,
            Title = viewModel.Title,
            NeedFeedback = viewModel.NeedFeedback,
            FeedbackNodeKey = viewModel.FeedbackNodeKey,
            Enabled = viewModel.Enabled
        };
    }

    public static ObservableCollection<S7NodeViewModel> ToViewModelCollection(List<S7PlcNode> nodes)
    {
        var collection = new ObservableCollection<S7NodeViewModel>();
        if (nodes != null)
            foreach (var node in nodes)
                collection.Add(ToViewModel(node));

        return collection;
    }

    public static List<S7PlcNode> ToNodeList(ObservableCollection<S7NodeViewModel> viewModels)
    {
        var list = new List<S7PlcNode>();
        if (viewModels != null)
            foreach (var vm in viewModels)
                list.Add(ToNode(vm));

        return list;
    }
}
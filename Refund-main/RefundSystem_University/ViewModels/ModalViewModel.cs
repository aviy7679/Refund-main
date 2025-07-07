namespace RefundSystem_University.ViewModels
{
    public class ModalViewModel
    {
        public string Type { get; set; }
        public string SaveButtonId { get; set; }
        public string SaveFunction { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool HideButtons { get; set; }
        public string SaveButtonText { get; set; }

        public string CloseTarget { get; set; }
    }
}
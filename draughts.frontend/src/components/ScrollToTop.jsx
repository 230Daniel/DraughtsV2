import { useLocation } from "react-router-dom";

export default function ScrollToTop(props) {
	// useLocation hook means this component always re-renders when page changes
	useLocation();
	// When the page changes, scroll to the top
	window.scrollTo(0, 0);
	// Don't render anything to the page
	return null;
}

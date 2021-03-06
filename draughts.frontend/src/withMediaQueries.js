import { useEffect, useState } from "react";

// Adds props isMobile and isDesktop to any component which requests the information.
// This component unfortunately has to use react hooks which are a lot harder to understand than Class components.

export function withMediaQueries(Component) {
	return function WithMediaQueriesComponent(props) {

		var [isMobile, setIsMobile] = useState(window.matchMedia("(max-width: 700px)").matches);

		useEffect(() => {

			const onChange = (e) => setIsMobile(e.matches);
			const media = window.matchMedia("(max-width: 700px)");
			media.addEventListener("change", onChange);

			return () => {
				media.removeEventListener("change", onChange);
			};

		});

		return <Component {...props} isMobile={isMobile} isDesktop={!isMobile} />;
	};
}

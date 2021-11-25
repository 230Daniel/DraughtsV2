import React from "react";
import { Link } from "react-router-dom";
import Expand from 'react-expand-animated';

import { withMediaQueries } from "../../withMediaQueries";

import styles from "./Navbar.module.css";

class Navbar extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			expanded: false
		};
	}

	render() {
		return (
			<div className={styles.container}>
				<div className={styles.bar}>
					<div className={styles.left}>
						<Link to="/" className={styles.brand}>
							<div className={styles.logo} />
							<span>Draughts</span>
						</Link>
						{/* If we're on a desktop screen always render the navbar items */}
						{this.props.isDesktop &&
							<div className={styles.items}>
								{this.renderItems()}
							</div>
						}
					</div>
					<div className={styles.right}>
						{/* If we're on a mobile screen render a hamburger button instead, when it's clicked toggle navbar expansion */}
						{this.props.isMobile &&
							<button className={styles.expandButton} onClick={(e) => { e.stopPropagation(); this.setState({ expanded: !this.state.expanded }); }}>
								<svg viewBox="0 0 100 70" width="40" height="40">
									<rect width="100" height="10"></rect>
									<rect y="30" width="100" height="10"></rect>
									<rect y="60" width="100" height="10"></rect>
								</svg>
							</button>
						}
					</div>
				</div>
				{/* If we're on a mobile screen render an Expand component which contains the navbar items as a column */}
				{this.props.isMobile &&
					<Expand open={this.state.expanded} styles={{ open: { opacity: '1 !important' }, close: { opacity: '1 !important' } }}>
						<div className={`${styles.items} ${styles.itemsColumn}`}>
							{this.renderItems()}
						</div>
					</Expand>
				}
			</div>
		);
	}

	renderItems() {
		return (
			<>
				<Link to="/play">Play</Link>
				<Link to="/play/local-multiplayer">Local</Link>
				<Link to="/play/online-multiplayer">Online</Link>
			</>
		);
	}

	componentDidMount() {
		// When the component mounts register the click event handler
		document.addEventListener("click", this.onClick);
	}

	componentWillUnmount() {
		// To avoid a memory leak, unregister the click event handler just before the component unmounts
		document.removeEventListener("click", this.onClick);
	}

	onClick = () => {
		// When the user clicks anywhere else on the page collapse the navbar
		this.setState({ expanded: false });
	};
}

// We use the withMediaQueries function so that information about the device is passed to this component
// It's necessary to decide whether to display the navbar in desktop or mobile mode
export default withMediaQueries(Navbar);

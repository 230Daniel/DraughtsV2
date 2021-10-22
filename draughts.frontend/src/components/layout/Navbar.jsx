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
						{this.props.isDesktop &&
							<div className={styles.items} onClick={(e) => { e.stopPropagation(); this.setState({ expanded: !this.state.expanded }); }}>
								{this.renderItems()}
							</div>
						}
					</div>
					<div className={styles.right}>
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
				{this.props.isMobile &&
					<Expand styles={{ open: { opacity: '1 !important' }, close: { opacity: '1 !important' } }} open={this.state.expanded}>
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
				<Link to="/play">Find a game</Link>
			</>
		);
	}

	componentDidMount() {
		document.addEventListener("click", this.onClick);
	}

	componentWillUnmount() {
		document.removeEventListener("click", this.onClick);
	}

	onClick = () => {
		this.setState({ expanded: false });
	};
}

export default withMediaQueries(Navbar);

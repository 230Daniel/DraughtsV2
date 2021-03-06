import React from "react";
import { Switch, Route } from "react-router";
import { Link } from "react-router-dom";

import LocalMultiplayer from "./LocalMultiplayer";
import OnlineMultiplayer from "./OnlineMultiplayer";
import Computer from "./Computer";

import styles from "./Play.Module.css";

export default class Play extends React.Component {
	render() {
		return (
			<Switch>
				<Route exact path="/play/local-multiplayer" component={LocalMultiplayer} />
				<Route exact path="/play/online-multiplayer" component={OnlineMultiplayer} />
				<Route exact path="/play/computer" component={Computer} />
				<Route>
					<div className={styles.container}>
						<div className={styles.row}>
							<Mode link="/play/local-multiplayer" name="Local Multiplayer" image="/local-multiplayer.png" />
							<Mode link="/play/online-multiplayer" name="Online Multiplayer" image="/online-multiplayer.png" />
							<Mode link="/play/computer" name="Computer" image="/computer.png" />
						</div>
					</div>
				</Route>
			</Switch>
		);
	}
}

class Mode extends React.Component {
	render() {
		return (
			<Link className={styles.mode} to={this.props.link}>
				<span>{this.props.name}</span>
				<div>
					<img src={this.props.image} alt="" />
				</div>
			</Link>
		);
	}
}

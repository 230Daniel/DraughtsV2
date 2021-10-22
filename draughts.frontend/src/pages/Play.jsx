import React from "react";
import { Redirect } from "react-router";

import styles from "./Play.Module.css";

export default class Play extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			redirect: null
		};
	}

	render() {
		// If the state has been set to redirect the user, redirect them
		if (this.state.redirect) {
			return (
				<Redirect to={this.state.redirect} />
			);
		}

		return (
			<div className={styles.container}>
				<form className={styles.form} onSubmit={(e) => this.createGame(e)}>
					<span className={styles.title}>Play Draughts</span>
					<button className={styles.button} type="submit">Create Game</button>
				</form>
			</div>
		);
	}

	async createGame(e) {

		// Prevent the form from submitting in the normal way
		e.preventDefault();

		// Send a request to the server to create a game, then redirect the user to the game page
		await window._connection.invoke("CREATE_GAME");
		this.setState({ redirect: "/game" });
	}
}

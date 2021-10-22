import React from "react";
import { Redirect } from "react-router";

import Form from "../components/form/Form";
import FormButton from "../components/form/FormButton";
import FormTitle from "../components/form/FormTitle";

export default class Play extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			redirect: null
		};
	}

	render() {
		if (this.state.redirect) {
			return (
				<Redirect to={this.state.redirect} />
			);
		}

		return (
			<Form onSubmit={() => this.createGame()}>
				<FormTitle>Play Draughts</FormTitle>
				<FormButton type="submit">Create Game</FormButton>
			</Form>
		);
	}

	async createGame() {
		await window._connection.invoke("CREATE_GAME");
		this.setState({ redirect: "/game" });
	}
}

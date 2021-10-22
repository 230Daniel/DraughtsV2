import React from "react";

import styles from "./Form.module.css";

export default class FormTitle extends React.Component {
	render() {
		return (
			<span className={`${styles.title}`}>{this.props.children}</span>
		);
	}
}
